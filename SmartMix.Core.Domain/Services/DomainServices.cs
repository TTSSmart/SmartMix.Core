using SmartMix.Core.Domain.Entities;
using SmartMix.Core.Domain.Events;
using SmartMix.Core.Domain.ValueObjects;

namespace SmartMix.Core.Domain.Services;

public enum BatchingStage { Dosing, Mixing, Completed, Failed }
public enum DosingMode { Auto, Manual }
public class MaterialVolume { public MaterialId MaterialId { get; init; } public string MaterialName { get; init; } = string.Empty; public Volume Volume { get; init; } public MaterialWeight TargetWeight { get; init; } }
public class DosingCommand { public MaterialId MaterialId { get; init; } public DosingMode Mode { get; init; } }
public class PlcDosingCommand { public int BunkerNumber { get; init; } public MaterialId MaterialId { get; init; } public MaterialWeight TargetWeight { get; init; } }
public class BatchingContext { public Application Application { get; init; } = null!; public Recipe Recipe { get; init; } = null!; public int MixerNumber { get; init; } public BatchingStage CurrentStage { get; set; } public MaterialId CurrentDosingMaterial { get; set; } public List<MaterialId> CompletedMaterials { get; } = []; public List<MaterialVolume> MaterialVolumes { get; init; } = []; public DateTime StartedAt { get; init; } = DateTime.UtcNow; }
public record BatchingResult(bool Success, string? ErrorMessage, BatchingContext? Context = null, PlcDosingCommand? Command = null, int ApplicationId = 0);

public sealed class RecipeCalculator
{
    public static List<MaterialVolume> CalculateMaterialVolumes(Recipe recipe, Volume totalVolume)
    {
        var structures = recipe.Structures ?? [];
        var total = structures.Sum(x => x.Percentage);
        return structures.Select(x => new MaterialVolume { MaterialId = x.ComponentId, MaterialName = x.ComponentName, TargetWeight = x.TargetWeight, Volume = Volume.FromCubicMeters(total == 0 ? 0 : totalVolume.CubicMeters * x.Percentage / total) }).ToList();
    }
    public static MaterialWeight CalculateHumidityCorrection(MaterialWeight dryWeight, Moisture target, Moisture actual) => actual.Percent >= target.Percent ? MaterialWeight.Zero : dryWeight * ((target.Fraction - actual.Fraction) / Math.Max(1m - target.Fraction, 0.0001m));
    public static Volume CalculateWaterDose(Volume _, Moisture target, Moisture current, MaterialWeight dryWeight) => Volume.FromLiters(CalculateHumidityCorrection(dryWeight, target, current).Kilograms);
    public static RecipeValidationResult ValidateRecipe(Recipe recipe)
    {
        var result = new RecipeValidationResult();
        if (recipe.Structures is not { Count: > 0 }) result.Errors.Add("Рецепт не содержит компонентов");
        else { var sum = recipe.Structures.Sum(x => x.Percentage); if (sum != 100m) result.Warnings.Add($"Сумма процентов компонентов: {sum}%"); }
        return result;
    }
}
public sealed class RecipeValidationResult { public List<string> Errors { get; } = []; public List<string> Warnings { get; } = []; public bool IsValid => Errors.Count == 0; public override string ToString() => string.Join("; ", Errors.Concat(Warnings)); }

public sealed class BatchingOrchestrator(IDomainEventPublisher eventPublisher)
{
    private readonly Dictionary<int, BatchingContext> _contexts = [];
    public BatchingResult StartBatch(Application application, int mixerNumber, Recipe recipe)
    {
        if (_contexts.ContainsKey(mixerNumber)) return new(false, "В смесителе уже выполняется замес");
        var context = new BatchingContext { Application = application, MixerNumber = mixerNumber, Recipe = recipe, CurrentStage = BatchingStage.Dosing, MaterialVolumes = RecipeCalculator.CalculateMaterialVolumes(recipe, application.Volume) };
        _contexts.Add(mixerNumber, context); eventPublisher.Publish(new BatchStartedEvent { ApplicationId = application.Id, MixerNumber = mixerNumber }); return new(true, null, context);
    }
    public BatchingResult ProcessDosingCommand(int mixerNumber, DosingCommand command)
    {
        if (!_contexts.TryGetValue(mixerNumber, out var context)) return new(false, "Замес не найден");
        var material = context.MaterialVolumes.FirstOrDefault(x => x.MaterialId == command.MaterialId);
        if (material is null) return new(false, "Материал отсутствует в рецепте");
        context.CurrentDosingMaterial = command.MaterialId;
        return new(true, null, context, new PlcDosingCommand { BunkerNumber = command.MaterialId.Value, MaterialId = command.MaterialId, TargetWeight = material.TargetWeight });
    }
    public BatchingResult CompleteBatch(int mixerNumber, bool success, string? error = null)
    {
        if (!_contexts.Remove(mixerNumber, out var context)) return new(false, "Активный замес не найден");
        eventPublisher.Publish(new BatchCompletedEvent { ApplicationId = context.Application.Id, MixerNumber = mixerNumber, Success = success }); return new(success, error, ApplicationId: context.Application.Id);
    }
    public BatchingContext? GetContext(int mixerNumber) => _contexts.GetValueOrDefault(mixerNumber);
    public IReadOnlyDictionary<int, BatchingContext> GetAllContexts() => _contexts;
}

public sealed class ConsumptionTracker(IDomainEventPublisher eventPublisher)
{
    private readonly List<(int Bunker, MaterialId Material, MaterialWeight Weight, int Application, int Batch, DateTime At)> _items = [];
    public void RecordConsumption(int bunker, MaterialId material, MaterialWeight weight, int application, int batch) { _items.Add((bunker, material, weight, application, batch, DateTime.UtcNow)); eventPublisher.Publish(new MaterialConsumedEvent { BunkerNumber = bunker, MaterialId = material.Value, Weight = weight }); }
    public MaterialWeight GetTotalConsumption(MaterialId material) => _items.Where(x => x.Material == material).Aggregate(MaterialWeight.Zero, (sum, x) => sum + x.Weight);
    public MaterialWeight GetBunkerConsumption(int bunker, MaterialId material) => _items.Where(x => x.Bunker == bunker && x.Material == material).Aggregate(MaterialWeight.Zero, (sum, x) => sum + x.Weight);
    public ConsumptionReport GetReport(DateTime from, DateTime to) => new() { From = from, To = to, Materials = _items.Where(x => x.At >= from && x.At <= to).GroupBy(x => x.Material).Select(g => new ConsumptionMaterial { MaterialId = g.Key, TotalConsumed = g.Aggregate(MaterialWeight.Zero, (sum, x) => sum + x.Weight), LastConsumption = g.Max(x => x.At), LastApplicationId = g.Last().Application, LastBatchNumber = g.Last().Batch }).ToList() };
    public void ResetCounters() { _items.Clear(); eventPublisher.Publish(new ConsumptionCountersResetEvent()); }
}

public sealed class AlarmProcessor { public IReadOnlyDictionary<int, AlarmState> GetActiveAlarms() => new Dictionary<int, AlarmState>(); }
public sealed class AlarmState { public Dictionary<int, Alarm> ActiveAlarms { get; } = []; }
public sealed class Alarm { public int Code { get; init; } public string Message { get; init; } = string.Empty; public DateTime RaisedAt { get; init; } public DateTime? AcknowledgedAt { get; init; } }
