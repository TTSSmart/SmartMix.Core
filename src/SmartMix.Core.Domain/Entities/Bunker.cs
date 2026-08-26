using SmartMix.Core.Domain.Enums;
using SmartMix.Core.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities;

/// <summary>
/// Бункер
/// </summary>
[DataContract]
public class Bunker : Entity<int>, ICloneable<Bunker>
{
    [DataMember]
    [Required]
    public BunkerNumber Number { get; set; }

    [DataMember]
    public bool IsOn { get; set; }

    [DataMember]
    public MaterialId ComponentId { get; set; }

    [DataMember]
    public string Title { get; set; } = string.Empty;

    [DataMember]
    public bool UseManualFore { get; set; }

    [DataMember]
    public MaterialWeight ManualFore { get; set; } = MaterialWeight.FromKg(50);

    [DataMember]
    public bool UseHumidity { get; set; }

    [DataMember]
    public Moisture Humidity { get; set; } = Moisture.Zero;

    [DataMember]
    public bool UseHumiditySensor { get; set; }

    [DataMember]
    public MaterialWeight MinDose { get; set; } = MaterialWeight.FromKg(10);

    [DataMember]
    public MaterialWeight Accuracy { get; set; } = MaterialWeight.FromKg(5);

    [DataMember]
    public decimal FlowPercent { get; set; } = 80;

    [DataMember]
    public TimeSpan LoadTime { get; set; } = TimeSpan.FromMinutes(5);

    // Runtime state (не сохраняется в БД)
    public MaterialWeight CurrentWeight { get; set; }
    public MaterialId CurrentMaterialId { get; set; }
    public bool IsActive { get; set; }
    public bool IsInAlarm { get; set; }
    public string AlarmMessage { get; set; } = string.Empty;
    public int Priority { get; set; }
    public int ErrorCode { get; set; }
    public bool HighLevel { get; set; }
    public bool LowLevel { get; set; }
    public bool FilterActive { get; set; }

    public Bunker Clone()
    {
        var clone = (Bunker)MemberwiseClone();
        return clone;
    }
}