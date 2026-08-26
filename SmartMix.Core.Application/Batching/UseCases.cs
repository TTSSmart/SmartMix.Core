using SmartMix.Core.Application.Abstractions;
using SmartMix.Core.Domain.Entities;
using SmartMix.Core.Domain.ValueObjects;
using SmartMix.Core.Domain.Events;
using SmartMix.Core.Domain.Services;
using SmartMix.Core.Domain.Specifications;

namespace SmartMix.Core.Application.Batching;

/// <summary>
/// Use Case: Запуск замеса
/// </summary>
public class StartBatchUseCase
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly BatchingOrchestrator _orchestrator;
    private readonly IDomainEventPublisher _eventPublisher;
    
    public StartBatchUseCase(
        IApplicationRepository applicationRepository,
        IRecipeRepository recipeRepository,
        BatchingOrchestrator orchestrator,
        IDomainEventPublisher eventPublisher)
    {
        _applicationRepository = applicationRepository;
        _recipeRepository = recipeRepository;
        _orchestrator = orchestrator;
        _eventPublisher = eventPublisher;
    }
    
    public async Task<StartBatchResult> ExecuteAsync(StartBatchCommand command, CancellationToken ct = default)
    {
        // 1. Получить заявку
        var application = await _applicationRepository.GetByIdWithLayersAsync(command.ApplicationId, ct);
        if (application == null)
            return StartBatchResult.Failure("Заявка не найдена");
        
        // 2. Проверить спецификацию
        var canStartSpec = new CanStartBatch();
        if (!canStartSpec.IsSatisfiedBy(application))
            return StartBatchResult.Failure(canStartSpec.GetFailureReason(application));
        
        // 3. Получить рецепт первого слоя
        var layer = application.Layers.FirstOrDefault();
        if (layer?.Recipe == null)
            return StartBatchResult.Failure("У заявки нет рецепта");
        
        var recipe = await _recipeRepository.GetByIdAsync(layer.Recipe.Id, ct);
        if (recipe == null)
            return StartBatchResult.Failure($"Рецепт {layer.Recipe.Id} не найден");
        
        // 4. Проверить рецепт
        var recipeSpec = new RecipeValidForProduction();
        if (!recipeSpec.IsSatisfiedBy(recipe))
            return StartBatchResult.Failure($"Рецепт невалиден: {recipeSpec.GetFailureReason(recipe)}");
        
        // 5. Запустить через оркестратор
        var batchResult = _orchestrator.StartBatch(application, command.MixerNumber, recipe);
        if (!batchResult.Success)
            return StartBatchResult.Failure(batchResult.ErrorMessage!);
        
        // 6. Обновить заявку
        application.IsRunning = true;
        application.StartTime = DateTime.UtcNow;
        application.BatchPhase = 1; // Reading data
        application.LastSaveBatchNum = 0;
        await _applicationRepository.UpdateAsync(application, ct);
        
        return StartBatchResult.Ok(batchResult.Context!);
    }
}

public record StartBatchCommand(int ApplicationId, int MixerNumber);
public record StartBatchResult(bool Success, string? ErrorMessage, BatchingContext? Context)
{
    public static StartBatchResult Ok(BatchingContext context) => new(true, null, context);
    public static StartBatchResult Failure(string error) => new(false, error, null);
}

/// <summary>
/// Use Case: Дозирование материала
/// </summary>
public class DoseMaterialUseCase
{
    private readonly BatchingOrchestrator _orchestrator;
    private readonly IPlcClient _plcClient;
    
    public DoseMaterialUseCase(BatchingOrchestrator orchestrator, IPlcClient plcClient)
    {
        _orchestrator = orchestrator;
        _plcClient = plcClient;
    }
    
    public async Task<DoseMaterialResult> ExecuteAsync(DoseMaterialCommand command, CancellationToken ct = default)
    {
        var result = _orchestrator.ProcessDosingCommand(command.MixerNumber, new DosingCommand
        {
            MaterialId = command.MaterialId,
            Mode = command.Mode
        });
        
        if (!result.Success)
            return DoseMaterialResult.Failure(result.ErrorMessage!);
        
        // Отправить команду в ПЛК
        var plcCommand = result.Command!;
        await SendPlcCommandAsync(plcCommand, ct);
        
        return DoseMaterialResult.Ok(plcCommand);
    }
    
    private async Task SendPlcCommandAsync(PlcDosingCommand command, CancellationToken ct)
    {
        // Маппинг команды на регистры ПЛК
        var nvi = PlcVarsPatterns.Nvi;
        var nci = PlcVarsPatterns.Nci;
        
        // Записать целевой вес в бункер
        await _plcClient.WriteSingleRegisterAsync(
            (ushort)nvi.BunkerTargetWeight(command.BunkerNumber), 
            (ushort)command.TargetWeight.Grams, ct);
        
        // Установить приоритет бункера
        await _plcClient.WriteSingleRegisterAsync(
            (ushort)nvi.BunkerPriority(command.BunkerNumber), 
            1, ct);
        
        // Команда на старт дозирования
        await _plcClient.WriteSingleRegisterAsync(
            (ushort)nvi.StartDosing(command.BunkerNumber), 
            1, ct);
    }
}

public record DoseMaterialCommand(int MixerNumber, int MaterialId, DosingMode Mode = DosingMode.Auto);
public record DoseMaterialResult(bool Success, string? ErrorMessage, PlcDosingCommand? Command)
{
    public static DoseMaterialResult Ok(PlcDosingCommand cmd) => new(true, null, cmd);
    public static DoseMaterialResult Failure(string error) => new(false, error, null);
}

/// <summary>
/// Use Case: Завершение замеса
/// </summary>
public class CompleteBatchUseCase
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly BatchingOrchestrator _orchestrator;
    private readonly ConsumptionTracker _consumptionTracker;
    private readonly IDomainEventPublisher _eventPublisher;
    
    public CompleteBatchUseCase(
        IApplicationRepository applicationRepository,
        BatchingOrchestrator orchestrator,
        ConsumptionTracker consumptionTracker,
        IDomainEventPublisher eventPublisher)
    {
        _applicationRepository = applicationRepository;
        _orchestrator = orchestrator;
        _consumptionTracker = consumptionTracker;
        _eventPublisher = eventPublisher;
    }
    
    public async Task<CompleteBatchResult> ExecuteAsync(CompleteBatchCommand command, CancellationToken ct = default)
    {
        var context = _orchestrator.GetContext(command.MixerNumber);
        if (context == null)
            return CompleteBatchResult.Failure("Активный замес не найден");
        
        var result = _orchestrator.CompleteBatch(command.MixerNumber, command.Success, command.ErrorMessage);
        
        if (result.Success)
        {
            var application = await _applicationRepository.GetByIdAsync(context.Application.Id, ct);
            if (application != null)
            {
                application.IsRunning = false;
                application.IsCompleted = command.Success;
                application.EndTime = DateTime.UtcNow;
                application.BatchPhase = 6; // Completed
                await _applicationRepository.UpdateAsync(application, ct);
            }
        }
        
        return result.Success
            ? CompleteBatchResult.Ok(result.ApplicationId)
            : CompleteBatchResult.Failure(result.ErrorMessage ?? "Ошибка завершения замеса");
    }
}

public record CompleteBatchCommand(int MixerNumber, bool Success, string? ErrorMessage = null);
public record CompleteBatchResult(bool Success, string? ErrorMessage, int ApplicationId)
{
    public static CompleteBatchResult Ok(int appId) => new(true, null, appId);
    public static CompleteBatchResult Failure(string error) => new(false, error, 0);
}

/// <summary>
/// Use Case: Получение статуса замеса
/// </summary>
public class GetBatchStatusUseCase
{
    private readonly BatchingOrchestrator _orchestrator;
    private readonly IApplicationRepository _applicationRepository;
    
    public GetBatchStatusUseCase(BatchingOrchestrator orchestrator, IApplicationRepository applicationRepository)
    {
        _orchestrator = orchestrator;
        _applicationRepository = applicationRepository;
    }
    
    public async Task<BatchStatusDto?> ExecuteAsync(int mixerNumber, CancellationToken ct = default)
    {
        var context = _orchestrator.GetContext(mixerNumber);
        if (context == null)
        {
            // Попробовать получить из БД
            var app = await _applicationRepository.GetByIdAsync(0, ct); // TODO: найти по смесителю
            return null;
        }
        
        return new BatchStatusDto
        {
            MixerNumber = mixerNumber,
            ApplicationId = context.Application.Id,
            RecipeName = context.Recipe.Name,
            Stage = context.CurrentStage.ToString(),
            CurrentMaterial = context.CurrentDosingMaterial.Value,
            CompletedMaterials = context.CompletedMaterials.Select(m => m.Value).ToList(),
            TotalMaterials = context.MaterialVolumes.Count,
            Progress = CalculateProgress(context),
            StartedAt = context.StartedAt,
            Elapsed = DateTime.UtcNow - context.StartedAt
        };
    }
    
    private double CalculateProgress(BatchingContext context)
    {
        if (context.MaterialVolumes.Count == 0) return 0;
        
        var completed = context.CompletedMaterials.Count;
        var current = context.CurrentDosingMaterial != MaterialId.Empty ? 0.5 : 0; // Приблизительно
        var total = context.MaterialVolumes.Count + 1; // + mixing
        
        return Math.Min(100, (completed + current) / (double)total * 100);
    }
}

public record BatchStatusDto
{
    public int MixerNumber { get; init; }
    public int ApplicationId { get; init; }
    public string RecipeName { get; init; } = string.Empty;
    public string Stage { get; init; } = string.Empty;
    public int CurrentMaterial { get; init; }
    public List<int> CompletedMaterials { get; init; } = new();
    public int TotalMaterials { get; init; }
    public double Progress { get; init; }
    public DateTime StartedAt { get; init; }
    public TimeSpan Elapsed { get; init; }
}
