using SmartMix.Core.Domain.Enums;
using SmartMix.Core.Domain.ValueObjects;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities.Mechanisms;

[DataContract]
public class Switch : Mechanism, ICloneable<Switch>
{
    public Switch() { Type = MechanicsType.Switch; }

    [DataMember]
    public SwitchConfig Config { get; set; } = new();

    [DataMember]
    public SwitchState State { get; set; } = new();

    public SwitchPosition Position => State.Position;
    public bool IsInTransition => State.IsInTransition;

    public override Switch Clone()
    {
        var clone = (Switch)MemberwiseClone();
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
public class SwitchConfig : ICloneable<SwitchConfig>
{
    [DataMember]
    public TimeSpan SwitchTime { get; set; } = TimeSpan.FromSeconds(10);

    [DataMember]
    public int LeftBunkerNumber { get; set; }

    [DataMember]
    public int RightBunkerNumber { get; set; }

    [DataMember]
    public int MotoHours { get; set; }

    public SwitchConfig Clone() => (SwitchConfig)MemberwiseClone();
}

[DataContract]
public class SwitchState : ICloneable<SwitchState>
{
    [DataMember]
    public SwitchPosition Position { get; set; }

    [DataMember]
    public bool IsInTransition { get; set; }

    [DataMember]
    public int MotoHours { get; set; }

    [DataMember]
    public int ErrorCode { get; set; }

    public SwitchState Clone() => (SwitchState)MemberwiseClone();

    public void UpdateFromRegisters(RegistersData registers, int number, SwitchConfig config)
    {
        var nvo = PlcVarsPatterns.Nvo;

        Position = (SwitchPosition)registers.GetInt(nvo.SwitchPosition(number));
        IsInTransition = registers.GetBool(nvo.SwitchIsMoving(number));
        MotoHours = registers.GetInt(nvo.SwitchMotoHours(number));
        ErrorCode = registers.GetInt(nvo.SwitchErrorCode(number));
    }
}

public enum SwitchPosition
{
    Unknown = 0,
    Left = 1,
    Right = 2,
    Middle = 3
}

[DataContract]
public class Kubel : Mechanism, ICloneable<Kubel>
{
    public Kubel() { Type = MechanicsType.Kubel; }

    [DataMember]
    public KubelConfig Config { get; set; } = new();

    [DataMember]
    public KubelState State { get; set; } = new();

    public KubelMode Mode => State.Mode;
    public int CurrentPost => State.CurrentPost;
    public int TargetPost => State.TargetPost;
    public bool IsMoving => State.IsMoving;

    public override Kubel Clone()
    {
        var clone = (Kubel)MemberwiseClone();
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
public class KubelConfig : ICloneable<KubelConfig>
{
    [DataMember]
    public int PostCount { get; set; } = 4;

    [DataMember]
    public TimeSpan OpenValveTime { get; set; } = TimeSpan.FromSeconds(5);

    [DataMember]
    public TimeSpan CloseValveTime { get; set; } = TimeSpan.FromSeconds(5);

    [DataMember]
    public TimeSpan UnloadingTime { get; set; } = TimeSpan.FromSeconds(30);

    [DataMember]
    public bool AutoUnload { get; set; }

    [DataMember]
    public bool AutoReturn { get; set; }

    [DataMember]
    public TimeSpan MoveTimePassBy { get; set; } = TimeSpan.FromSeconds(15);

    [DataMember]
    public TimeSpan MoveTimeSlowDown { get; set; } = TimeSpan.FromSeconds(10);

    [DataMember]
    public bool UseViber { get; set; }

    [DataMember]
    public TimeSpan ViberPulseTime { get; set; } = TimeSpan.FromMilliseconds(500);

    [DataMember]
    public TimeSpan ViberDelayTime { get; set; } = TimeSpan.FromMilliseconds(500);

    [DataMember]
    public int MotoHours { get; set; }

    public KubelConfig Clone() => (KubelConfig)MemberwiseClone();
}

[DataContract]
public class KubelState : ICloneable<KubelState>
{
    [DataMember]
    public KubelMode Mode { get; set; }

    [DataMember]
    public int CurrentPost { get; set; }

    [DataMember]
    public int TargetPost { get; set; }

    [DataMember]
    public bool IsMoving { get; set; }

    [DataMember]
    public bool GateOpen { get; set; }

    [DataMember]
    public bool ViberActive { get; set; }

    [DataMember]
    public int MotoHours { get; set; }

    [DataMember]
    public int ErrorCode { get; set; }

    [DataMember]
    public bool IsLocked { get; set; }

    public KubelState Clone() => (KubelState)MemberwiseClone();

    public void UpdateFromRegisters(RegistersData registers, int number, KubelConfig config)
    {
        var nvo = PlcVarsPatterns.Nvo;
        var nvi = PlcVarsPatterns.Nvi;

        Mode = (KubelMode)registers.GetInt(nvo.KubelMode(number));
        CurrentPost = registers.GetInt(nvo.KubelCurrentPost(number));
        TargetPost = registers.GetInt(nvi.KubelTargetPost(number));
        IsMoving = registers.GetBool(nvo.KubelIsMoving(number));
        GateOpen = registers.GetBool(nvo.KubelGateOpen(number));
        ViberActive = registers.GetBool(nvo.KubelViberActive(number));
        MotoHours = registers.GetInt(nvo.KubelMotoHours(number));
        ErrorCode = registers.GetInt(nvo.KubelErrorCode(number));
        IsLocked = registers.GetBool(nvo.KubelIsLocked(number));
    }
}

public enum KubelMode
{
    Manual = 0,
    Auto = 1,
    Unloading = 2,
    Moving = 3,
    Error = 4
}

[DataContract]
public class Sensor : Mechanism, ICloneable<Sensor>
{
    public Sensor() { Type = MechanicsType.Sensor; }

    [DataMember]
    public SensorConfig Config { get; set; } = new();

    [DataMember]
    public SensorState State { get; set; } = new();

    public Moisture CurrentMoisture => State.CurrentMoisture;
    public Temperature CurrentTemperature => State.CurrentTemperature;
    public MaterialWeight CurrentWeight => State.CurrentWeight;
    public bool IsCalibrated => State.IsCalibrated;

    public override Sensor Clone()
    {
        var clone = (Sensor)MemberwiseClone();
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
public class SensorConfig : ICloneable<SensorConfig>
{
    [DataMember]
    public SensorType Type { get; set; }

    [DataMember]
    public int AiNumber { get; set; }

    [DataMember]
    public decimal CoefK { get; set; } = 1.0m;

    [DataMember]
    public decimal CoefB { get; set; } = 0.0m;

    [DataMember]
    public Moisture MinMoisture { get; set; } = Moisture.Zero;

    [DataMember]
    public Moisture MaxMoisture { get; set; } = Moisture.FromPercent(100);

    [DataMember]
    public Temperature MinTemperature { get; set; } = Temperature.FromCelsius(-20);

    [DataMember]
    public Temperature MaxTemperature { get; set; } = Temperature.FromCelsius(80);

    [DataMember]
    public MaterialWeight MinWeight { get; set; } = MaterialWeight.Zero;

    [DataMember]
    public MaterialWeight MaxWeight { get; set; } = MaterialWeight.FromKg(10000);

    [DataMember]
    public int FilterValue { get; set; } = 5;

    [DataMember]
    public bool UseHumidityCorrection { get; set; }

    public SensorConfig Clone() => (SensorConfig)MemberwiseClone();
}

[DataContract]
public class SensorState : ICloneable<SensorState>
{
    [DataMember]
    public Moisture CurrentMoisture { get; set; }

    [DataMember]
    public Temperature CurrentTemperature { get; set; }

    [DataMember]
    public MaterialWeight CurrentWeight { get; set; }

    [DataMember]
    public int RawValue { get; set; }

    [DataMember]
    public bool IsCalibrated { get; set; }

    [DataMember]
    public DateTime LastCalibrationDate { get; set; }

    [DataMember]
    public int ErrorCode { get; set; }

    public SensorState Clone() => (SensorState)MemberwiseClone();

    public void UpdateFromRegisters(RegistersData registers, int number, SensorConfig config)
    {
        var nvo = PlcVarsPatterns.Nvo;

        CurrentMoisture = registers.GetFloat(nvo.SensorMoisture(number)).ToMoisture();
        CurrentTemperature = registers.GetFloat(nvo.SensorTemperature(number)).ToTemperature();
        CurrentWeight = registers.GetFloat(nvo.SensorWeight(number)).ToMaterialWeight();
        RawValue = registers.GetInt(nvo.SensorRawValue(number));
        IsCalibrated = registers.GetBool(nvo.SensorIsCalibrated(number));
        ErrorCode = registers.GetInt(nvo.SensorErrorCode(number));
    }
}

public enum SensorType
{
    Moisture = 1,
    Temperature = 2,
    Weight = 3,
    Level = 4,
    Pressure = 5,
    Flow = 6,
    Current = 7
}