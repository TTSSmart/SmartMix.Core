using SmartMix.Core.Domain.Enums;
using SmartMix.Core.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities;

/// <summary>
/// Заявка на производство
/// </summary>
[DataContract]
public class Application : Entity<int>, ICloneable<Application>
{
    [DataMember]
    [MaxLength(50)]
    public string WayBill { get; set; } = string.Empty;

    [DataMember]
    public int ClientId { get; set; }

    [DataMember]
    public int CarId { get; set; }

    [DataMember]
    public Volume Volume { get; set; }

    [DataMember]
    public Volume FactVolume { get; set; }

    [DataMember]
    public Volume VolumeCurrent { get; set; } = Volume.Zero;

    [DataMember]
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;

    [DataMember]
    public DateTime? StartTime { get; set; }

    [DataMember]
    public DateTime? EndTime { get; set; }

    [DataMember]
    public bool IsCompleted { get; set; }

    [DataMember]
    public bool IsDeleted { get; set; }

    [DataMember]
    public int CreatorId { get; set; }

    [DataMember]
    public int UserId { get; set; }

    [DataMember]
    public MixerNumber MixerNumber { get; set; } = MixerNumber.Empty;

    [DataMember]
    public int Order { get; set; }

    [DataMember]
    public int BatchPhase { get; set; }

    [DataMember]
    public bool TrainMode { get; set; }

    [DataMember]
    public bool IsSpeedApp { get; set; }

    [DataMember]
    public int LastSaveBatchNum { get; set; }

    [DataMember]
    public bool IsRunning { get; set; }

    [DataMember]
    public bool IsEditLock { get; set; }

    [DataMember]
    public int ProductId { get; set; }

    [DataMember]
    public List<ApplicationLayer> Layers { get; set; } = new();

    [DataMember]
    public int LastLayerIndex { get; set; }

    [DataMember]
    public int UnloadingPoint { get; set; }

    [DataMember]
    public MaterialWeight CorrectWaterPanel { get; set; } = MaterialWeight.Zero;

    [DataMember]
    public bool StartAuto { get; set; }

    [DataMember]
    public ApplicationCalibrationMixHum CalibMixerHum { get; set; } = new();

    // Computed properties
    public ApplicationStatus Status => BatchPhase switch
    {
        <= 7 => (ApplicationStatus)BatchPhase,
        _ => ApplicationStatus.Complete
    };

    public bool IsActive => IsRunning && !IsCompleted && !IsDeleted;

    public Application Clone()
    {
        var clone = (Application)MemberwiseClone();
        clone.Layers = Layers.Select(l => l.Clone()).ToList();
        return clone;
    }
}

/// <summary>
/// Слой заявки (рецепт в заявке)
/// </summary>
[DataContract]
public class ApplicationLayer : Entity<int>, ICloneable<ApplicationLayer>
{
    [DataMember]
    public int ApplicationId { get; set; }

    [DataMember]
    public int Number { get; set; }

    [DataMember]
    public RecipeId RecipeId { get; set; }

    [DataMember]
    public Volume Volume { get; set; }

    [DataMember]
    public Volume CurVolume { get; set; } = Volume.Zero;

    [DataMember]
    public Volume FactVolume { get; set; } = Volume.Zero;

    [DataMember]
    public LayerStatus Status { get; set; } = LayerStatus.Wait;

    [DataMember]
    public int BatcherPerformed { get; set; }

    [DataMember]
    public Recipe? Recipe { get; set; }

    public ApplicationLayer Clone()
    {
        var clone = (ApplicationLayer)MemberwiseClone();
        return clone;
    }
}

/// <summary>
/// Калибровка влажности смесителя для заявки
/// </summary>
[DataContract]
public class ApplicationCalibrationMixHum : ICloneable<ApplicationCalibrationMixHum>
{
    [DataMember]
    public MaterialWeight TargetMoisture { get; set; } = MaterialWeight.Zero;

    [DataMember]
    public MaterialWeight CurrentMoisture { get; set; } = MaterialWeight.Zero;

    [DataMember]
    public MaterialWeight Correction { get; set; } = MaterialWeight.Zero;

    public ApplicationCalibrationMixHum Clone() => (ApplicationCalibrationMixHum)MemberwiseClone();
}