using SmartMix.Core.Domain.Enums;
using SmartMix.Core.Domain.ValueObjects;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities.Mechanisms;

/// <summary>
/// Базовый класс механизма
/// </summary>
[DataContract]
[KnownType(typeof(Doser))]
[KnownType(typeof(Mixer))]
[KnownType(typeof(Skip))]
[KnownType(typeof(Accumulator))]
[KnownType(typeof(BunkerMechanism))]
[KnownType(typeof(Viber))]
[KnownType(typeof(Valve))]
[KnownType(typeof(TransportLine))]
[KnownType(typeof(Compressor))]
[KnownType(typeof(Switch))]
[KnownType(typeof(Kubel))]
[KnownType(typeof(Sensor))]
public abstract class Mechanism : Entity<int>, IMechanic, ICloneable<Mechanism>
{
    [DataMember]
    public MechanicsType Type { get; protected set; }

    [DataMember]
    public int Number { get; set; }

    [DataMember]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [DataMember]
    public bool IsEnabled { get; set; } = true;

    [DataMember]
    public bool IsInAlarm { get; set; }

    [DataMember]
    [MaxLength(500)]
    public string AlarmMessage { get; set; } = string.Empty;

    [DataMember]
    public DateTime LastUpdate { get; set; }

    public abstract Mechanism Clone();
    public abstract void UpdateFromRegisters(RegistersData registers);
    public abstract void ValidateConfig();
}

[DataContract]
public class Doser : Mechanism, ICloneable<Doser>
{
    public Doser() { Type = MechanicsType.Doser; }

    [DataMember]
    public DoserConfig Config { get; set; } = new();

    [DataMember]
    public DoserState State { get; set; } = new();

    [DataMember]
    public List<AttachedBunker> AttachedBunkers { get; set; } = new();

    public MaterialWeight CurrentWeight => State.CurrentWeight;
    public MaterialWeight TargetWeight => State.TargetWeight;
    public DoserStatus Status => State.Status;
    public bool IsDosing => Status == DoserStatus.Dosing;
    public bool IsUnloading => Status == DoserStatus.Unloading;
    public bool IsReady => Status == DoserStatus.Ready;

    public override Doser Clone()
    {
        var clone = (Doser)MemberwiseClone();
        clone.Config = Config.Clone();
        clone.State = State.Clone();
        clone.AttachedBunkers = AttachedBunkers.Select(b => b.Clone()).ToList();
        return clone;
    }

    public override void UpdateFromRegisters(RegistersData registers)
    {
        State.UpdateFromRegisters(registers, Number, Config);
    }

    public override void ValidateConfig()
    {
        if (Config.MaxWeight.IsZero) throw new InvalidOperationException($"Doser {Number}: MaxWeight не задан");
        if (Config.AiNumber == 0) throw new InvalidOperationException($"Doser {Number}: AI номер не задан");
    }
}

[DataContract]
public class DoserConfig : ICloneable<DoserConfig>
{
    [DataMember]
    public int AiNumber { get; set; }

    [DataMember]
    public MaterialWeight MaxWeight { get; set; }

    [DataMember]
    public MaterialWeight MinWeight { get; set; }

    [DataMember]
    public MaterialWeight Accuracy { get; set; }

    [DataMember]
    public TimeSpan MaxDosingTime { get; set; } = TimeSpan.FromMinutes(5);

    [DataMember]
    public TimeSpan MinUnloadTime { get; set; } = TimeSpan.FromSeconds(2);

    [DataMember]
    public TimeSpan MaxUnloadTime { get; set; } = TimeSpan.FromMinutes(2);

    [DataMember]
    public decimal CoefK { get; set; } = 1.0m;

    [DataMember]
    public decimal CoefB { get; set; } = 0.0m;

    [DataMember]
    public int FilterValue { get; set; } = 5;

    [DataMember]
    public decimal CorrectPercent { get; set; } = 0;

    [DataMember]
    public TimeSpan RelaxTime { get; set; } = TimeSpan.FromSeconds(1);

    [DataMember]
    public TimeSpan LoadTime { get; set; } = TimeSpan.FromSeconds(30);

    [DataMember]
    public bool AutoSetZero { get; set; }

    [DataMember]
    public DoserUnloadMode UnloadMode { get; set; }

    [DataMember]
    public MaterialWeight Fore { get; set; } = MaterialWeight.FromKg(50);

    [DataMember]
    public bool UseTwoStepDosing { get; set; }

    [DataMember]
    public MaterialWeight FastDosingPercentage { get; set; } = MaterialWeight.Zero;

    public DoserConfig Clone() => (DoserConfig)MemberwiseClone();
}

[DataContract]
public class DoserState : ICloneable<DoserState>
{
    [DataMember]
    public MaterialWeight CurrentWeight { get; set; }

    [DataMember]
    public MaterialWeight TargetWeight { get; set; }

    [DataMember]
    public MaterialWeight NalipWeight { get; set; }

    [DataMember]
    public DoserStatus Status { get; set; }

    [DataMember]
    public int BatchPhase { get; set; }

    [DataMember]
    public int CurrentBunkerNumber { get; set; }

    [DataMember]
    public MaterialId CurrentMaterialId { get; set; }

    [DataMember]
    public bool NeedWash { get; set; }

    [DataMember]
    public bool IsCalibrating { get; set; }

    [DataMember]
    public int ErrorCode { get; set; }

    [DataMember]
    public DateTime LastDoseTime { get; set; }

    public DoserState Clone() => (DoserState)MemberwiseClone();

    public void UpdateFromRegisters(RegistersData registers, int number, DoserConfig config)
    {
        var nvo = PlcVarsPatterns.Nvo;
        var nci = PlcVarsPatterns.Nci;

        CurrentWeight = registers.GetFloat(nvo.DoserCurrentWeight(number)).ToMaterialWeight();
        TargetWeight = registers.GetFloat(nvo.DoserTargetWeight(number)).ToMaterialWeight();
        NalipWeight = registers.GetFloat(nci.DoserStikyMass(number)).ToMaterialWeight();
        Status = (DoserStatus)registers.GetInt(nci.DoserBatchPhase(number));
        BatchPhase = registers.GetInt(nci.DoserBatchPhase(number));
        NeedWash = registers.GetBool(nvi.DoserNeedWash(number));
        ErrorCode = registers.GetInt(nvo.DoserErrorCode(number));
    }
}

[DataContract]
public class AttachedBunker : ICloneable<AttachedBunker>
{
    [DataMember]
    public BunkerNumber BunkerNumber { get; set; }

    [DataMember]
    public MaterialId MaterialId { get; set; }

    [DataMember]
    public MaterialWeight MaxDose { get; set; }

    [DataMember]
    public int Priority { get; set; }

    [DataMember]
    public bool IsActive { get; set; } = true;

    public AttachedBunker Clone() => (AttachedBunker)MemberwiseClone();
}