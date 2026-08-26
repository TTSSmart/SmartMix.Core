using SmartMix.Core.Domain.Enums;
using SmartMix.Core.Domain.ValueObjects;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities.Mechanisms;

[DataContract]
public class Mixer : Mechanism, ICloneable<Mixer>
{
    public Mixer() { Type = MechanicsType.Mixer; }

    [DataMember]
    public MixerConfig Config { get; set; } = new();

    [DataMember]
    public MixerState State { get; set; } = new();

    [DataMember]
    public List<MixerGate> Gates { get; set; } = new();

    public Volume Capacity => Config.Volume;
    public MixerStatus Status => State.Status;
    public bool IsMixing => Status == MixerStatus.Mixing;
    public bool IsUnloading => Status == MixerStatus.Unloading;
    public bool IsReady => Status == MixerStatus.Ready;

    public override Mixer Clone()
    {
        var clone = (Mixer)MemberwiseClone();
        clone.Config = Config.Clone();
        clone.State = State.Clone();
        clone.Gates = Gates.Select(g => g.Clone()).ToList();
        return clone;
    }

    public override void UpdateFromRegisters(RegistersData registers)
    {
        State.UpdateFromRegisters(registers, Number, Config);
        foreach (var gate in Gates)
            gate.UpdateFromRegisters(registers, Number);
    }

    public override void ValidateConfig()
    {
        if (Config.Volume.IsZero) throw new InvalidOperationException($"Mixer {Number}: Volume не задан");
        if (Config.GateCount <= 0) throw new InvalidOperationException($"Mixer {Number}: GateCount должен быть > 0");
    }
}

[DataContract]
public class MixerConfig : ICloneable<MixerConfig>
{
    [DataMember]
    public Volume Volume { get; set; } = Volume.FromCubicMeters(3);

    [DataMember]
    public TimeSpan MixTime { get; set; } = TimeSpan.FromSeconds(60);

    [DataMember]
    public bool UseCommonMixTime { get; set; }

    [DataMember]
    public TimeSpan UnloadTime { get; set; } = TimeSpan.FromSeconds(10);

    [DataMember]
    public TimeSpan ExtraUnloadTime { get; set; } = TimeSpan.FromSeconds(3);

    [DataMember]
    public MixerUnloadMode UnloadMode { get; set; } = MixerUnloadMode.Automatic;

    [DataMember]
    public int GateCount { get; set; } = 1;

    [DataMember]
    public int DefaultGateNumber { get; set; } = 1;

    [DataMember]
    public MaterialWeight MaxCurrent { get; set; } = MaterialWeight.FromKg(50);

    [DataMember]
    public MaterialWeight MaxCurrentLoad { get; set; } = MaterialWeight.FromKg(40);

    [DataMember]
    public TimeSpan CurrentLoadDelay { get; set; } = TimeSpan.FromSeconds(5);

    [DataMember]
    public bool EnableCurrentCheck { get; set; }

    [DataMember]
    public bool EnableHatchCheck { get; set; }

    [DataMember]
    public TimeSpan AutoOffDelay { get; set; } = TimeSpan.FromMinutes(30);

    [DataMember]
    public bool AutoOffEnabled { get; set; }

    [DataMember]
    public int LubricatorWorkMode { get; set; }

    [DataMember]
    public TimeSpan LubricatorWorkTime { get; set; }

    [DataMember]
    public TimeSpan LubricatorDelayTime { get; set; }

    public MixerConfig Clone() => (MixerConfig)MemberwiseClone();
}

[DataContract]
public class MixerState : ICloneable<MixerState>
{
    [DataMember]
    public MixerStatus Status { get; set; }

    [DataMember]
    public int BatchPhase { get; set; }

    [DataMember]
    public Volume CurrentVolume { get; set; }

    [DataMember]
    public MaterialWeight CurrentWeight { get; set; }

    [DataMember]
    public Moisture CurrentMoisture { get; set; }

    [DataMember]
    public int CurrentRecipeId { get; set; }

    [DataMember]
    public int CurrentBatchNumber { get; set; }

    [DataMember]
    public int ApplicationNumber { get; set; }

    [DataMember]
    public TimeSpan RemainingMixTime { get; set; }

    [DataMember]
    public TimeSpan RemainingUnloadTime { get; set; }

    [DataMember]
    public TimeSpan RemainingStartTime { get; set; }

    [DataMember]
    public bool IsPaused { get; set; }

    [DataMember]
    public bool NeedWash { get; set; }

    [DataMember]
    public int ErrorCode { get; set; }

    [DataMember]
    public decimal MotorCurrent { get; set; }

    [DataMember]
    public int MotoHours { get; set; }

    public MixerState Clone() => (MixerState)MemberwiseClone();

    public void UpdateFromRegisters(RegistersData registers, int number, MixerConfig config)
    {
        var nvo = PlcVarsPatterns.Nvo;
        var nci = PlcVarsPatterns.Nci;

        Status = (MixerStatus)registers.GetInt(nci.MixerBatchPhase(number));
        BatchPhase = registers.GetInt(nci.MixerBatchPhase(number));
        CurrentVolume = registers.GetFloat(nvo.MixerCurrentVolume(number)).ToVolume();
        CurrentWeight = registers.GetFloat(nvo.MixerCurrentWeight(number)).ToMaterialWeight();
        CurrentMoisture = registers.GetFloat(nvo.MixerMoisture(number)).ToMoisture();
        CurrentRecipeId = registers.GetInt(nvo.MixerRecipeNumber(number));
        CurrentBatchNumber = registers.GetInt(nvo.MixerBatchNumber(number));
        ApplicationNumber = registers.GetInt(nvo.MixerApplicationNumber(number));
        RemainingMixTime = TimeSpan.FromSeconds(registers.GetInt(nvo.MixerRemainingMixTime(number)));
        RemainingUnloadTime = TimeSpan.FromSeconds(registers.GetInt(nvo.MixerRemainingUnloadTime(number)));
        RemainingStartTime = TimeSpan.FromSeconds(registers.GetInt(nvo.MixerRemainingStartTime(number)));
        IsPaused = registers.GetBool(nvo.MixerIsPaused(number));
        MotorCurrent = registers.GetFloat(nvo.MixerMotorCurrent(number));
        MotoHours = registers.GetInt(nvo.MixerMotoHours(number));
        ErrorCode = registers.GetInt(nvo.MixerErrorCode(number));
    }
}

[DataContract]
public class MixerGate : ICloneable<MixerGate>
{
    [DataMember]
    public int GateNumber { get; set; }

    [DataMember]
    public int PulseCount { get; set; } = 1;

    [DataMember]
    public TimeSpan PulseTime { get; set; } = TimeSpan.FromMilliseconds(500);

    [DataMember]
    public TimeSpan DelayTime { get; set; } = TimeSpan.FromMilliseconds(200);

    [DataMember]
    public bool DontCloseAfterImpulse { get; set; }

    [DataMember]
    public bool UseSettingsFromRecipe { get; set; }

    [DataMember]
    public TimeSpan SensorDelayToActive { get; set; } = TimeSpan.FromSeconds(2);

    [DataMember]
    public GateStatus Status { get; set; }

    public MixerGate Clone() => (MixerGate)MemberwiseClone();

    public void UpdateFromRegisters(RegistersData registers, int mixerNumber)
    {
        var nvo = PlcVarsPatterns.Nvo;
        var nvi = PlcVarsPatterns.Nvi;

        Status = (GateStatus)registers.GetInt(nvo.MixerGateStatus(mixerNumber, GateNumber));
        PulseCount = registers.GetInt(nvi.MixerGatePulseCount(mixerNumber, GateNumber));
        PulseTime = TimeSpan.FromMilliseconds(registers.GetInt(nvi.MixerGatePulseTime(mixerNumber, GateNumber)));
        DelayTime = TimeSpan.FromMilliseconds(registers.GetInt(nvi.MixerGateDelayTime(mixerNumber, GateNumber)));
        DontCloseAfterImpulse = registers.GetBool(nvi.MixerGateDontCloseAfterImpulse(mixerNumber, GateNumber));
        UseSettingsFromRecipe = registers.GetBool(nvi.MixerGateUseFromRecipe(mixerNumber, GateNumber));
        SensorDelayToActive = TimeSpan.FromSeconds(registers.GetInt(nvi.MixerGateSensorDelay(mixerNumber, GateNumber)));
    }
}