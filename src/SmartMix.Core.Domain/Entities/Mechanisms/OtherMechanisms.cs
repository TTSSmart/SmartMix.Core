using SmartMix.Core.Domain.Enums;
using SmartMix.Core.Domain.ValueObjects;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities.Mechanisms;

[DataContract]
public class Skip : Mechanism, ICloneable<Skip>
{
    public Skip() { Type = MechanicsType.Skip; }

    [DataMember]
    public SkipConfig Config { get; set; } = new();

    [DataMember]
    public SkipState State { get; set; } = new();

    public MaterialWeight MaxWeight { get; set; } = MaterialWeight.FromKg(2000);
    public SkipStatus Status => State.Status;
    public bool IsLoading => Status == SkipStatus.Loading;
    public bool IsUnloading => Status == SkipStatus.Unloading;
    public bool IsMoving => Status == SkipStatus.MovingUp || Status == SkipStatus.MovingDown;
    public bool IsReady => Status == SkipStatus.Ready;

    public override Skip Clone()
    {
        var clone = (Skip)MemberwiseClone();
        clone.Config = Config.Clone();
        clone.State = State.Clone();
        return clone;
    }

    public override void UpdateFromRegisters(RegistersData registers)
    {
        State.UpdateFromRegisters(registers, Number, Config);
    }
}

[DataContract]
public class SkipConfig : ICloneable<SkipConfig>
{
    [DataMember]
    public TimeSpan TopMoveTime { get; set; } = TimeSpan.FromSeconds(15);

    [DataMember]
    public TimeSpan BottomMoveTime { get; set; } = TimeSpan.FromSeconds(15);

    [DataMember]
    public TimeSpan UnloadTimeMin { get; set; } = TimeSpan.FromSeconds(3);

    [DataMember]
    public bool UseBottomSensor { get; set; } = true;

    [DataMember]
    public TimeSpan StartBeforeBatchEnd { get; set; } = TimeSpan.FromSeconds(5);

    [DataMember]
    public MaterialWeight MaxWeight { get; set; } = MaterialWeight.FromKg(2000);

    [DataMember]
    public int ViberNumber { get; set; }

    [DataMember]
    public bool UseViber { get; set; }

    [DataMember]
    public TimeSpan ViberWorkTime { get; set; } = TimeSpan.FromSeconds(5);

    [DataMember]
    public TimeSpan ViberPulseTime { get; set; } = TimeSpan.FromMilliseconds(500);

    [DataMember]
    public TimeSpan ViberDelayTime { get; set; } = TimeSpan.FromMilliseconds(500);

    public SkipConfig Clone() => (SkipConfig)MemberwiseClone();
}

[DataContract]
public class SkipState : ICloneable<SkipState>
{
    [DataMember]
    public SkipStatus Status { get; set; }

    [DataMember]
    public int BatchPhase { get; set; }

    [DataMember]
    public MaterialWeight CurrentWeight { get; set; }

    [DataMember]
    public MaterialWeight BunkerWeight { get; set; }

    [DataMember]
    public int CurrentBunkerNumber { get; set; }

    [DataMember]
    public MaterialId CurrentMaterialId { get; set; }

    [DataMember]
    public int ApplicationNumber { get; set; }

    [DataMember]
    public int BatchNumber { get; set; }

    [DataMember]
    public TimeSpan RemainingTimeToUnload { get; set; }

    [DataMember]
    public TimeSpan RemainingUnloadTime { get; set; }

    [DataMember]
    public bool IsPaused { get; set; }

    [DataMember]
    public int ErrorCode { get; set; }

    [DataMember]
    public int MotoHours { get; set; }

    public SkipState Clone() => (SkipState)MemberwiseClone();

    public void UpdateFromRegisters(RegistersData registers, int number, SkipConfig config)
    {
        var nvo = PlcVarsPatterns.Nvo;
        var nci = PlcVarsPatterns.Nci;

        Status = (SkipStatus)registers.GetInt(nci.SkipBatchPhase(number));
        BatchPhase = registers.GetInt(nci.SkipBatchPhase(number));
        CurrentWeight = registers.GetFloat(nvo.SkipCurrentWeight(number)).ToMaterialWeight();
        BunkerWeight = registers.GetFloat(nvo.SkipBunkerWeight(number)).ToMaterialWeight();
        CurrentBunkerNumber = registers.GetInt(nvo.SkipCurrentBunker(number));
        ApplicationNumber = registers.GetInt(nvo.SkipApplicationNumber(number));
        BatchNumber = registers.GetInt(nvo.SkipBatchNumber(number));
        RemainingTimeToUnload = TimeSpan.FromSeconds(registers.GetInt(nvo.SkipRemainingStartUnloadTime(number)));
        RemainingUnloadTime = TimeSpan.FromSeconds(registers.GetInt(nvo.SkipRemainingUnloadTime(number)));
        IsPaused = registers.GetBool(nvo.SkipIsPaused(number));
        ErrorCode = registers.GetInt(nvo.SkipErrorCode(number));
        MotoHours = registers.GetInt(nvo.SkipMotoHours(number));
    }
}

[DataContract]
public class Accumulator : Mechanism, ICloneable<Accumulator>
{
    public Accumulator() { Type = MechanicsType.Accumulator; }

    [DataMember]
    public AccumulatorConfig Config { get; set; } = new();

    [DataMember]
    public AccumulatorState State { get; set; } = new();

    public AccumulatorStatus Status => State.Status;
    public bool IsUnloading => Status == AccumulatorStatus.Unloading;
    public bool IsReady => Status == AccumulatorStatus.Ready;

    public override Accumulator Clone()
    {
        var clone = (Accumulator)MemberwiseClone();
        clone.Config = Config.Clone();
        clone.State = State.Clone();
        return clone;
    }

    public override void UpdateFromRegisters(RegistersData registers)
    {
        State.UpdateFromRegisters(registers, Number, Config);
    }
}

[DataContract]
public class AccumulatorConfig : ICloneable<AccumulatorConfig>
{
    [DataMember]
    public MaterialWeight MaxWeight { get; set; } = MaterialWeight.FromKg(5000);

    [DataMember]
    public TimeSpan UnloadTimeMin { get; set; } = TimeSpan.FromSeconds(5);

    [DataMember]
    public TimeSpan UnloadTimeMax { get; set; } = TimeSpan.FromMinutes(2);

    [DataMember]
    public MaterialWeight NalipWeight { get; set; } = MaterialWeight.FromKg(20);

    [DataMember]
    public bool UseWeightUnload { get; set; }

    [DataMember]
    public TimeSpan RelaxTime { get; set; } = TimeSpan.FromSeconds(2);

    [DataMember]
    public TimeSpan AutoStopTime { get; set; } = TimeSpan.FromMinutes(10);

    [DataMember]
    public bool AutoStopEnabled { get; set; }

    [DataMember]
    public int MotoHours { get; set; }

    public AccumulatorConfig Clone() => (AccumulatorConfig)MemberwiseClone();
}

[DataContract]
public class AccumulatorState : ICloneable<AccumulatorState>
{
    [DataMember]
    public AccumulatorStatus Status { get; set; }

    [DataMember]
    public int BatchPhase { get; set; }

    [DataMember]
    public MaterialWeight CurrentWeight { get; set; }

    [DataMember]
    public MaterialWeight NalipWeight { get; set; }

    [DataMember]
    public int ApplicationNumber { get; set; }

    [DataMember]
    public int BatchNumber { get; set; }

    [DataMember]
    public TimeSpan RemainingTimeToUnload { get; set; }

    [DataMember]
    public TimeSpan RemainingUnloadTime { get; set; }

    [DataMember]
    public bool IsPaused { get; set; }

    [DataMember]
    public int ErrorCode { get; set; }

    [DataMember]
    public int MotoHours { get; set; }

    public AccumulatorState Clone() => (AccumulatorState)MemberwiseClone();

    public void UpdateFromRegisters(RegistersData registers, int number, AccumulatorConfig config)
    {
        var nvo = PlcVarsPatterns.Nvo;
        var nci = PlcVarsPatterns.Nci;

        Status = (AccumulatorStatus)registers.GetInt(nci.AccumulatorBatchPhase(number));
        BatchPhase = registers.GetInt(nci.AccumulatorBatchPhase(number));
        CurrentWeight = registers.GetFloat(nvo.AccumulatorCurrentWeight(number)).ToMaterialWeight();
        NalipWeight = registers.GetFloat(nci.AccumWeightNalip(number)).ToMaterialWeight();
        ApplicationNumber = registers.GetInt(nvo.AccumulatorApplicationNumber(number));
        BatchNumber = registers.GetInt(nvo.AccumulatorBatchNumber(number));
        RemainingTimeToUnload = TimeSpan.FromSeconds(registers.GetInt(nvo.AccumulatorRemainingStartUnloadTime(number)));
        RemainingUnloadTime = TimeSpan.FromSeconds(registers.GetInt(nvo.AccumulatorRemainingUnloadTime(number)));
        IsPaused = registers.GetBool(nvo.AccumulatorIsPaused(number));
        ErrorCode = registers.GetInt(nvo.AccumulatorErrorCode(number));
        MotoHours = registers.GetInt(nvo.AccumulatorMotoHours(number));
    }
}

[DataContract]
public class BunkerMechanism : Mechanism, ICloneable<BunkerMechanism>
{
    public BunkerMechanism() { Type = MechanicsType.Bunker; }

    [DataMember]
    public BunkerConfig Config { get; set; } = new();

    [DataMember]
    public BunkerState State { get; set; } = new();

    [DataMember]
    public BunkerNumber BunkerNumber { get; set; }

    public MaterialWeight CurrentWeight => State.CurrentWeight;
    public MaterialId CurrentMaterial => State.CurrentMaterialId;
    public bool IsActive => State.IsActive;
    public bool IsOn => Config.IsOn;
    public bool IsInAlarm => State.IsInAlarm;

    public override BunkerMechanism Clone()
    {
        var clone = (BunkerMechanism)MemberwiseClone();
        clone.Config = Config.Clone();
        clone.State = State.Clone();
        return clone;
    }

    public override void UpdateFromRegisters(RegistersData registers)
    {
        State.UpdateFromRegisters(registers, (int)BunkerNumber, Config);
    }
}

[DataContract]
public class BunkerConfig : ICloneable<BunkerConfig>
{
    [DataMember]
    public bool IsOn { get; set; }

    [DataMember]
    public MaterialId ComponentId { get; set; }

    [DataMember]
    [MaxLength(100)]
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

    [DataMember]
    public int ViberNumber { get; set; }

    [DataMember]
    public int AerationNumber { get; set; }

    [DataMember]
    public int RotaryLatchNumber { get; set; }

    [DataMember]
    public int HighLevelSensor { get; set; }

    [DataMember]
    public int LowLevelSensor { get; set; }

    [DataMember]
    public int FilterSensor { get; set; }

    public BunkerConfig Clone() => (BunkerConfig)MemberwiseClone();
}

[DataContract]
public class BunkerState : ICloneable<BunkerState>
{
    [DataMember]
    public MaterialWeight CurrentWeight { get; set; }

    [DataMember]
    public MaterialId CurrentMaterialId { get; set; }

    [DataMember]
    public bool IsActive { get; set; }

    [DataMember]
    public bool IsInAlarm { get; set; }

    [DataMember]
    [MaxLength(500)]
    public string AlarmMessage { get; set; } = string.Empty;

    [DataMember]
    public MaterialWeight ConsumptionWeight { get; set; }

    [DataMember]
    public int AutoLoadCount { get; set; }

    [DataMember]
    public int ManualLoadCount { get; set; }

    [DataMember]
    public MaterialWeight CorrectWeight { get; set; }

    [DataMember]
    public MaterialWeight NeedWeight { get; set; }

    [DataMember]
    public int Priority { get; set; }

    [DataMember]
    public int ErrorCode { get; set; }

    [DataMember]
    public bool HighLevel { get; set; }

    [DataMember]
    public bool LowLevel { get; set; }

    [DataMember]
    public bool FilterActive { get; set; }

    public BunkerState Clone() => (BunkerState)MemberwiseClone();

    public void UpdateFromRegisters(RegistersData registers, int number, BunkerConfig config)
    {
        var nvo = PlcVarsPatterns.Nvo;
        var nci = PlcVarsPatterns.Nci;
        var conf = PlcVarsPatterns.Conf;

        CurrentWeight = registers.GetFloat(nvo.BunkerCurrentWeight(number)).ToMaterialWeight();
        CurrentMaterialId = new MaterialId(registers.GetInt(nvo.BunkerCurrentMaterial(number)));
        IsActive = registers.GetBool(nci.BunkerUse(number));
        IsInAlarm = registers.GetBool(nvo.BunkerAlarm(number));
        ConsumptionWeight = registers.GetFloat(nvo.BunkerConsumptionWeight(number)).ToMaterialWeight();
        AutoLoadCount = registers.GetInt(nvo.BunkerAutoCount(number));
        ManualLoadCount = registers.GetInt(nvo.BunkerManualCount(number));
        CorrectWeight = registers.GetFloat(nvo.BunkerCorrectWeight(number)).ToMaterialWeight();
        NeedWeight = registers.GetFloat(nvo.BunkerNeedWeight(number)).ToMaterialWeight();
        Priority = registers.GetInt(nvo.BunkerPriority(number));
        HighLevel = registers.GetBool(nvo.BunkerHighLevel(number));
        LowLevel = registers.GetBool(nvo.BunkerLowLevel(number));
        FilterActive = registers.GetBool(nvo.BunkerFilterActive(number));
    }
}