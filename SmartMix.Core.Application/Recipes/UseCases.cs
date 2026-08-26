using SmartMix.Core.Application.Abstractions;
using SmartMix.Core.Domain.Entities;
using SmartMix.Core.Domain.ValueObjects;
using SmartMix.Core.Domain.Events;
using SmartMix.Core.Domain.Services;
using SmartMix.Core.Domain.Specifications;

namespace SmartMix.Core.Application.Recipes;

/// <summary>
/// Use Case: Создание рецепта
/// </summary>
public class CreateRecipeUseCase
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly IComponentRepository _componentRepository;
    private readonly IDomainEventPublisher _eventPublisher;
    
    public CreateRecipeUseCase(
        IRecipeRepository recipeRepository,
        IComponentRepository componentRepository,
        IDomainEventPublisher eventPublisher)
    {
        _recipeRepository = recipeRepository;
        _componentRepository = componentRepository;
        _eventPublisher = eventPublisher;
    }
    
    public async Task<CreateRecipeResult> ExecuteAsync(CreateRecipeCommand command, CancellationToken ct = default)
    {
        // Валидация компонентов
        foreach (var structure in command.Structures)
        {
            var component = await _componentRepository.GetByIdAsync(structure.ComponentId, ct);
            if (component == null)
                return CreateRecipeResult.Failure($"Компонент {structure.ComponentId} не найден");
        }
        
        // Создать рецепт
        var recipe = new Recipe
        {
            Name = command.Name,
            Consider = command.Consider,
            UseAutoCorrection = command.UseAutoCorrection,
            UserId = command.UserId,
            EditDate = DateTime.UtcNow,
            RecipeCategory = new RecipeCategory { Id = command.CategoryId },
            RecipeTimesSet = new RecipeTimesSet { Id = command.TimeSetId },
            RecipeMixerSet = new RecipeMixerSet { Id = command.MixerSetId },
            Structures = command.Structures.Select(s => new RecipeStructure
            {
                ComponentId = s.ComponentId,
                ComponentName = s.ComponentName,
                Percentage = s.Percentage,
                TargetWeight = MaterialWeight.FromKg(s.TargetWeightKg),
                Correct = s.Correct
            }).ToList()
        };
        
        // Валидация рецепта
        var validation = RecipeCalculator.ValidateRecipe(recipe);
        if (!validation.IsValid)
            return CreateRecipeResult.Failure($"Рецепт невалиден: {validation}");
        
        // Сохранить
        recipe.Id = await _recipeRepository.CreateAsync(recipe, ct);
        
        // Публиковать событие
        _eventPublisher.Publish(new RecipeCreatedEvent
        {
            RecipeId = recipe.Id,
            RecipeName = recipe.Name,
            CreatedBy = command.UserId
        });
        
        return CreateRecipeResult.Ok(recipe);
    }
}

public record CreateRecipeCommand(
    string Name,
    bool Consider,
    bool UseAutoCorrection,
    int UserId,
    int CategoryId,
    int TimeSetId,
    int MixerSetId,
    List<RecipeStructureDto> Structures);

public record RecipeStructureDto(
    int ComponentId,
    string ComponentName,
    decimal Percentage,
    decimal TargetWeightKg,
    decimal Correct);

public record CreateRecipeResult(bool Success, string? ErrorMessage, Recipe? Recipe)
{
    public static CreateRecipeResult Ok(Recipe recipe) => new(true, null, recipe);
    public static CreateRecipeResult Failure(string error) => new(false, error, null);
}

/// <summary>
/// Use Case: Обновление рецепта
/// </summary>
public class UpdateRecipeUseCase
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly IComponentRepository _componentRepository;
    private readonly IDomainEventPublisher _eventPublisher;
    
    public UpdateRecipeUseCase(
        IRecipeRepository recipeRepository,
        IComponentRepository componentRepository,
        IDomainEventPublisher eventPublisher)
    {
        _recipeRepository = recipeRepository;
        _componentRepository = componentRepository;
        _eventPublisher = eventPublisher;
    }
    
    public async Task<UpdateRecipeResult> ExecuteAsync(UpdateRecipeCommand command, CancellationToken ct = default)
    {
        var recipe = await _recipeRepository.GetByIdAsync(command.RecipeId, ct);
        if (recipe == null)
            return UpdateRecipeResult.Failure("Рецепт не найден");
        
        // Запомнить изменения для события
        var changes = new Dictionary<string, object>();
        if (recipe.Name != command.Name) changes["Name"] = command.Name;
        if (recipe.Consider != command.Consider) changes["Consider"] = command.Consider;
        if (recipe.UseAutoCorrection != command.UseAutoCorrection) changes["UseAutoCorrection"] = command.UseAutoCorrection;
        
        // Обновить поля
        recipe.Name = command.Name;
        recipe.Consider = command.Consider;
        recipe.UseAutoCorrection = command.UseAutoCorrection;
        recipe.UserId = command.UserId;
        recipe.EditDate = DateTime.UtcNow;
        
        if (command.Structures != null)
        {
            recipe.Structures = command.Structures.Select(s => new RecipeStructure
            {
                ComponentId = s.ComponentId,
                ComponentName = s.ComponentName,
                Percentage = s.Percentage,
                TargetWeight = MaterialWeight.FromKg(s.TargetWeightKg),
                Correct = s.Correct
            }).ToList();
            changes["Structures"] = "Updated";
        }
        
        // Валидация
        var validation = RecipeCalculator.ValidateRecipe(recipe);
        if (!validation.IsValid)
            return UpdateRecipeResult.Failure($"Рецепт невалиден: {validation}");
        
        await _recipeRepository.UpdateAsync(recipe, ct);
        
        _eventPublisher.Publish(new RecipeUpdatedEvent
        {
            RecipeId = recipe.Id,
            RecipeName = recipe.Name,
            UpdatedBy = command.UserId,
            Changes = changes
        });
        
        return UpdateRecipeResult.Ok(recipe);
    }
}

public record UpdateRecipeCommand(
    int RecipeId,
    string Name,
    bool Consider,
    bool UseAutoCorrection,
    int UserId,
    List<RecipeStructureDto>? Structures = null);

public record UpdateRecipeResult(bool Success, string? ErrorMessage, Recipe? Recipe)
{
    public static UpdateRecipeResult Ok(Recipe recipe) => new(true, null, recipe);
    public static UpdateRecipeResult Failure(string error) => new(false, error, null);
}

/// <summary>
/// Use Case: Расчет материалов для заявки
/// </summary>
public class CalculateRecipeMaterialsUseCase
{
    public CalculateRecipeMaterialsUseCase() { }
    
    public CalculateMaterialsResult Execute(CalculateMaterialsCommand command)
    {
        var materialVolumes = RecipeCalculator.CalculateMaterialVolumes(command.Recipe, command.TotalVolume);
        
        return new CalculateMaterialsResult
        {
            Success = true,
            Materials = materialVolumes,
            TotalVolume = command.TotalVolume
        };
    }
}

public record CalculateMaterialsCommand(Recipe Recipe, Volume TotalVolume);
public record CalculateMaterialsResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public List<MaterialVolume> Materials { get; init; } = new();
    public Volume TotalVolume { get; init; }
    
    public static CalculateMaterialsResult Ok(List<MaterialVolume> materials, Volume totalVolume) 
        => new() { Success = true, Materials = materials, TotalVolume = totalVolume };
    public static CalculateMaterialsResult Failure(string error) 
        => new() { Success = false, ErrorMessage = error };
}
