using SmartMix.Core.Application.Abstractions;
using SmartMix.Core.Domain.Entities;
using SmartMix.Core.Domain.ValueObjects;
using SmartMix.Core.Domain.Events;
using SmartMix.Core.Domain.Specifications;

namespace SmartMix.Core.Application.Bunkers;

/// <summary>
/// Use Case: Установка компонента в бункер
/// </summary>
public class SetBunkerComponentUseCase
{
    private readonly IBunkerRepository _bunkerRepository;
    private readonly IComponentRepository _componentRepository;
    private readonly IDomainEventPublisher _eventPublisher;
    
    public SetBunkerComponentUseCase(
        IBunkerRepository bunkerRepository,
        IComponentRepository componentRepository,
        IDomainEventPublisher eventPublisher)
    {
        _bunkerRepository = bunkerRepository;
        _componentRepository = componentRepository;
        _eventPublisher = eventPublisher;
    }
    
    public async Task<SetBunkerComponentResult> ExecuteAsync(SetBunkerComponentCommand command, CancellationToken ct = default)
    {
        var bunker = await _bunkerRepository.GetByNumberAsync(command.BunkerNumber, command.LineNumber, ct);
        if (bunker == null)
            return SetBunkerComponentResult.Failure("Бункер не найден");
        
        var component = await _componentRepository.GetByIdAsync(command.MaterialId, ct);
        if (component == null)
            return SetBunkerComponentResult.Failure("Компонент не найден");
        
        var oldMaterialId = bunker.ComponentId;
        bunker.ComponentId = command.MaterialId;
        
        await _bunkerRepository.UpdateAsync(bunker, ct);
        
        _eventPublisher.Publish(new BunkerComponentChangedEvent
        {
            BunkerNumber = command.BunkerNumber,
            OldMaterialId = oldMaterialId,
            NewMaterialId = command.MaterialId,
            ChangedBy = command.UserId
        });
        
        return SetBunkerComponentResult.Ok(bunker);
    }
}

public record SetBunkerComponentCommand(int BunkerNumber, int LineNumber, int MaterialId, int UserId);
public record SetBunkerComponentResult(bool Success, string? ErrorMessage, Bunker? Bunker)
{
    public static SetBunkerComponentResult Ok(Bunker bunker) => new(true, null, bunker);
    public static SetBunkerComponentResult Failure(string error) => new(false, error, null);
}

/// <summary>
/// Use Case: Включение/выключение бункера
/// </summary>
public class ToggleBunkerUseCase
{
    private readonly IBunkerRepository _bunkerRepository;
    private readonly IDomainEventPublisher _eventPublisher;
    
    public ToggleBunkerUseCase(IBunkerRepository bunkerRepository, IDomainEventPublisher eventPublisher)
    {
        _bunkerRepository = bunkerRepository;
        _eventPublisher = eventPublisher;
    }
    
    public async Task<ToggleBunkerResult> ExecuteAsync(ToggleBunkerCommand command, CancellationToken ct = default)
    {
        var bunker = await _bunkerRepository.GetByNumberAsync(command.BunkerNumber, command.LineNumber, ct);
        if (bunker == null)
            return ToggleBunkerResult.Failure("Бункер не найден");
        
        bunker.IsOn = command.IsOn;
        await _bunkerRepository.UpdateAsync(bunker, ct);
        
        _eventPublisher.Publish(new BunkerStateChangedEvent
        {
            BunkerNumber = command.BunkerNumber,
            IsActive = command.IsOn,
            ChangedBy = command.UserId
        });
        
        return ToggleBunkerResult.Ok(bunker);
    }
}

public record ToggleBunkerCommand(int BunkerNumber, int LineNumber, bool IsOn, int UserId);
public record ToggleBunkerResult(bool Success, string? ErrorMessage, Bunker? Bunker)
{
    public static ToggleBunkerResult Ok(Bunker bunker) => new(true, null, bunker);
    public static ToggleBunkerResult Failure(string error) => new(false, error, null);
}

/// <summary>
/// Use Case: Калибровка бункера
/// </summary>
public class CalibrateBunkerUseCase
{
    private readonly IBunkerRepository _bunkerRepository;
    private readonly IPlcClient _plcClient;
    
    public CalibrateBunkerUseCase(IBunkerRepository bunkerRepository, IPlcClient plcClient)
    {
        _bunkerRepository = bunkerRepository;
        _plcClient = plcClient;
    }
    
    public async Task<CalibrateBunkerResult> ExecuteAsync(CalibrateBunkerCommand command, CancellationToken ct = default)
    {
        var bunker = await _bunkerRepository.GetByNumberAsync(command.BunkerNumber, command.LineNumber, ct);
        if (bunker == null)
            return CalibrateBunkerResult.Failure("Бункер не найден");
        
        // Отправить команду калибровки в ПЛК
        var nvi = PlcVarsPatterns.Nvi;
        
        // Записать эталонный вес
        await _plcClient.WriteSingleRegisterAsync(
            (ushort)nvi.BunkerCalibWeight(command.BunkerNumber),
            (ushort)command.ReferenceWeight.Grams, ct);
        
        // Команда на калибровку
        await _plcClient.WriteSingleRegisterAsync(
            (ushort)nvi.BunkerCalibCommand(command.BunkerNumber),
            1, ct);
        
        return CalibrateBunkerResult.Ok();
    }
}

public record CalibrateBunkerCommand(int BunkerNumber, int LineNumber, MaterialWeight ReferenceWeight);
public record CalibrateBunkerResult(bool Success, string? ErrorMessage)
{
    public static CalibrateBunkerResult Ok() => new(true, null);
    public static CalibrateBunkerResult Failure(string error) => new(false, error);
}
