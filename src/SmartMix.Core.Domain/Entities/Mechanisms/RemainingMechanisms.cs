using SmartMix.Core.Domain.Enums;
using SmartMix.Core.Domain.ValueObjects;
using System.Runtime.Serialization;

namespace SmartMix.Core.Domain.Entities.Mechanisms;

[DataContract]
public class Viber : Mechanism, ICloneable<Viber>
{
    public Viber() { Type = MechanicsType.Viber; }

    [DataMember]
    public ViberConfig Config { get; set; } = new();

    [DataMember]
    public ViberState State { get; set; } = new();

    public VibrationAerationMode Mode => Config.Mode;
    public bool IsWorking => State.IsWorking;

    public override Viber Clone()
    {
        var clone = (Viber)MemberwiseClone();
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
public class ViberConfig : ICloneable<ViberConfig>
{
    [DataMember]
    public VibrationAerationMode Mode { get; set; } = VibrationAerationMode.Vibration;

    [DataMember]
    public MaterialWeight StartWeight { get; set; } = MaterialWeight.FromKg(100);

    [DataMember]
    public TimeSpan WorkTime { get; set; } = TimeSpan.FromSeconds(5);

    [DataMember]
    public TimeSpan PulseTime { get; set; } = TimeSpan.FromMilliseconds(500);

    [DataMember]
    public TimeSpan DelayTime { get; set; } = TimeSpan.FromMilliseconds(500);

    [DataMember]
    public bool IsPermanentMode { get; set; }

    [DataMember]
    public int MotoHours { get; set; }

    public ViberConfig Clone() => (ViberConfig)MemberwiseClone();
}

[DataContract]
public class ViberState : ICloneable<ViberState>
{
    [DataMember]
    public bool IsWorking { get; set; }

    [DataMember]
    public VibrationAerationMode CurrentMode { get; set; }

    [DataMember]
    public int MotoHours { get; set; }

    [DataMember]
    public int ErrorCode { get; set; }

    public ViberState Clone() => (ViberState)MemberwiseClone();

    public void UpdateFromRegisters(RegistersData registers, int number, ViberConfig config)
    {
        var nvo = PlcVarsPatterns.Nvo;

        IsWorking = registers.GetBool(nvo.ViberIsWorking(number));
        CurrentMode = (VibrationAerationMode)registers.GetInt(nvo.ViberCurrentMode(number));
        MotoHours = registers.GetInt(nvo.ViberMotoHours(number));
        ErrorCode = registers.GetInt(nvo.ViberErrorCode(number));
    }
}

[DataContract]
public class Valve : Mechanism, ICloneable<Valve>
{
    public Valve() { Type = MechanicsType.Valve; }

    [DataMember]
    public ValveConfig Config { get; set; } = new();

    [DataMember]
    public ValveState State { get; set; } = new();

    public ValveUnloadMode UnloadMode => Config.UnloadMode;
    public bool IsOpen => State.IsOpen;
    public bool IsInAlarm => State.IsInAlarm;

    public override Valve Clone()
    {
        var clone = (Valve)MemberwiseClone();
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
public class ValveConfig : ICloneable<ValveConfig>
{
    [DataMember]
    public ValveUnloadMode UnloadMode { get; set; } = ValveUnloadMode.Automatic;

    [DataMember]
    public TimeSpan OpenTime { get; set; } = TimeSpan.FromSeconds(5);

    [DataMember]
    public TimeSpan CloseTime { get; set; } = TimeSpan.FromSeconds(5);

    [DataMember]
    public TimeSpan ImpulseTime { get; set; } = TimeSpan.FromMilliseconds(500);

    [DataMember]
    public TimeSpan TimeToOpenAlarm { get; set; } = TimeSpan.FromSeconds(10);

    [DataMember]
    public TimeSpan TimeToCloseAlarm { get; set; } = TimeSpan.FromSeconds(10);

    [DataMember]
    public TimeSpan WorkingTime { get; set; } = TimeSpan.FromSeconds(30);

    [DataMember]
    public int MotoHours { get; set; }

    [DataMember]
    public bool Inverted { get; set; }

    public ValveConfig Clone() => (ValveConfig)MemberwiseClone();
}

[DataContract]
public class ValveState : ICloneable<ValveState>
{
    [DataMember]
    public bool IsOpen { get; set; }

    [DataMember]
    public bool IsInAlarm { get; set; }

    [DataMember]
    public ValveStatus Status { get; set; }

    [DataMember]
    public int MotoHours { get; set; }

    [DataMember]
    public int ErrorCode { get; set; }

    [DataMember]
    public TimeSpan OpenDuration { get; set; }

    public ValveState Clone() => (ValveState)MemberwiseClone();

    public void UpdateFromRegisters(RegistersData registers, int number, ValveConfig config)
    {
        var nvo = PlcVarsPatterns.Nvo;

        IsOpen = registers.GetBool(nvo.ValveIsOpen(number));
        IsInAlarm = registers.GetBool(nvo.ValveAlarm(number));
        Status = (ValveStatus)registers.GetInt(nvo.ValveStatus(number));
        MotoHours = registers.GetInt(nvo.ValveMotoHours(number));
        ErrorCode = registers.GetInt(nvo.ValveErrorCode(number));
        OpenDuration = TimeSpan.FromSeconds(registers.GetInt(nvo.ValveOpenDuration(number)));
    }
}

[DataContract]
public class TransportLine : Mechanism, ICloneable<TransportLine>
{
    public TransportLine() { Type = MechanicsType.TransportLine; }

    [DataMember]
    public TransportLineConfig Config { get; set; } = new();

    [DataMember]
    public TransportLineState State { get; set; } = new();

    public bool IsRunning => State.IsRunning;
    public bool IsAutoStopEnabled => Config.AutoStopEnabled;

    public override TransportLine Clone()
    {
        var clone = (TransportLine)MemberwiseClone();
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
public class TransportLineConfig : ICloneable<TransportLineConfig>
{
    [DataMember]
    public TimeSpan WarmUpTime { get; set; } = TimeSpan.FromSeconds(10);

    [DataMember]
    public TimeSpan AutoStopTime { get; set; } = TimeSpan.FromMinutes(5);

    [DataMember]
    public bool AutoStopEnabled { get; set; } = true;

    [DataMember]
    public int MotoHours { get; set; }

    public TransportLineConfig Clone() => (TransportLineConfig)MemberwiseClone();
}

[DataContract]
public class TransportLineState : ICloneable<TransportLineState>
{
    [DataMember]
    public bool IsRunning { get; set; }

    [DataMember]
    public int MotoHours { get; set; }

    [DataMember]
    public int ErrorCode { get; set; }

    [DataMember]
    public TimeSpan RunningTime { get; set; }

    public TransportLineState Clone() => (TransportLineState)MemberwiseClone();

    public void UpdateFromRegisters(RegistersData registers, int number, TransportLineConfig config)
    {
        var nvo = PlcVarsPatterns.Nvo;

        IsRunning = registers.GetBool(nvo.TransportLineIsRunning(number));
        MotoHours = registers.GetInt(nvo.TransportLineMotoHours(number));
        ErrorCode = registers.GetInt(nvo.TransportLineErrorCode(number));
        RunningTime = TimeSpan.FromSeconds(registers.GetInt(nvo.TransportLineRunningTime(number)));
    }
}

[DataContract]
public class Compressor : Mechanism, ICloneable<Compressor>
{
    public Compressor() { Type = MechanicsType.Compressor; }

    [DataMember]
    public CompressorConfig Config { get; set; } = new();

    [DataMember]
    public CompressorState State { get; set; } = new();

    public bool IsRunning => State.IsRunning;
    public bool IsLowPressure => State.IsLowPressure;

    public override Compressor Clone()
    {
        var clone = (Compressor)MemberwiseClone();
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
public class CompressorConfig : ICloneable<CompressorConfig>
{
    [DataMember]
    public MaterialWeight MinPressure { get; set; } = MaterialWeight.FromKg(6); // bar * 100

    [DataMember]
    public int MotoHours { get; set; }

    public CompressorConfig Clone() => (CompressorConfig)MemberwiseClone();
}

[DataContract]
public class CompressorState : ICloneable<CompressorState>
{
    [DataMember]
    public bool IsRunning { get; set; }

    [DataMember]
    public bool IsLowPressure { get; set; }

    [DataMember]
    public MaterialWeight CurrentPressure { get; set; }

    [DataMember]
    public int MotoHours { get; set; }

    [DataMember]
    public int ErrorCode { get; set; }

    public CompressorState Clone() => (CompressorState)MemberwiseClone();

    public void UpdateFromRegisters(RegistersData registers, int number, CompressorConfig config)
    {
        var nvo = PlcVarsPatterns.Nvo;

        IsRunning = registers.GetBool(nvo.CompressorIsRunning(number));
        IsLowPressure = registers.GetBool(nvo.CompressorLowPressure(number));
        CurrentPressure = registers.GetFloat(nvo.CompressorPressure(number)).ToMaterialWeight();
        MotoHours = registers.GetInt(nvo.CompressorMotoHours(number));
        ErrorCode = registers.GetInt(nvo.CompressorErrorCode(number));
    }
}