using SmartMix.Core.Domain.ValueObjects;

namespace SmartMix.Core.Domain.Entities;

public enum ApplicationStatus { Created, Running, Completed, Failed }
public enum LayerStatus { Pending, Running, Completed, Failed }
public enum ComponentType { Unknown, Cement, Inert, Water, Additive, Bitumen }

public class Application
{
    public int Id { get; set; }
    public string WayBill { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public int CarId { get; set; }
    public int ProductId { get; set; }
    public Volume Volume { get; set; }
    public Volume FactVolume { get; set; }
    public Volume VolumeCurrent { get; set; }
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsRunning { get; set; }
    public bool IsEditLock { get; set; }
    public int UserId { get; set; }
    public int CreatorId { get; set; }
    public int MixerNumber { get; set; }
    public int Order { get; set; }
    public int BatchPhase { get; set; }
    public int LastSaveBatchNum { get; set; }
    public int LineNumber { get; set; }
    public List<LayerApplication> Layers { get; set; } = [];
    public Client? Client { get; set; }
    public Car? Car { get; set; }
    public Product? Product { get; set; }
}

public class LayerApplication { public int Id { get; set; } public int Number { get; set; } public Volume Volume { get; set; } public LayerStatus Status { get; set; } public Recipe? Recipe { get; set; } }
public class Recipe { public int Id { get; set; } public string Name { get; set; } = string.Empty; public bool Consider { get; set; } = true; public bool UseAutoCorrection { get; set; } public int UserId { get; set; } public DateTime EditDate { get; set; } public RecipeCategory? RecipeCategory { get; set; } public RecipeTimesSet? RecipeTimesSet { get; set; } public RecipeMixerSet? RecipeMixerSet { get; set; } public List<RecipeStructure> Structures { get; set; } = []; public int CategoryId => RecipeCategory?.Id ?? 0; public int TimeSetId => RecipeTimesSet?.Id ?? 0; public int MixerSetId => RecipeMixerSet?.Id ?? 0; }
public class RecipeStructure { public int Id { get; set; } public MaterialId ComponentId { get; set; } public string ComponentName { get; set; } = string.Empty; public decimal Percentage { get; set; } public MaterialWeight TargetWeight { get; set; } public decimal Correct { get; set; } public Component? Component { get; set; } }
public class RecipeCategory { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
public class RecipeTimesSet { public int Id { get; set; } }
public class RecipeMixerSet { public int Id { get; set; } }
public class Component { public int Id { get; set; } public string Name { get; set; } = string.Empty; public ComponentType Type { get; set; } public int IdType { get; set; } public Component Clone() => (Component)MemberwiseClone(); }
public class Bunker { public int Id { get; set; } public int Number { get; set; } public int LineNumber { get; set; } public bool IsOn { get; set; } public bool IsActive { get; set; } public MaterialId ComponentId { get; set; } public decimal CurrentWeightKg { get; set; } }
public class User { public int Id { get; set; } public string Username { get; set; } = string.Empty; public string PasswordHash { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public string Role { get; set; } = string.Empty; public bool IsActive { get; set; } = true; public DateTime CreatedAt { get; set; } = DateTime.UtcNow; }
public class Client { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
public class Car { public int Id { get; set; } public string Number { get; set; } = string.Empty; public string Name { get => Number; set => Number = value; } public string Model { get; set; } = string.Empty; public string Driver { get; set; } = string.Empty; public decimal Volume { get; set; } }
public class Product { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
public class AutoStartSignal { public int Id { get; set; } public int LineNumber { get; set; } public int SensorNumber { get; set; } public bool IsActive { get; set; } }
public enum StartRequestDecision { Accept, Reject }
public class AutoStartRejection { public int ApplicationId { get; set; } public string Reason { get; set; } = string.Empty; }
public class ConsumptionReport { public DateTime From { get; set; } public DateTime To { get; set; } public List<ConsumptionMaterial> Materials { get; set; } = []; public Dictionary<int, Dictionary<MaterialId, MaterialWeight>> BunkerBreakdown { get; set; } = []; }
public class ConsumptionMaterial { public MaterialId MaterialId { get; set; } public MaterialWeight TotalConsumed { get; set; } public DateTime LastConsumption { get; set; } public int LastApplicationId { get; set; } public int LastBatchNumber { get; set; } }
public class ApplicationReport { }
public class BunkerConsumptionReport { }
