namespace SmartMix.Core.Application.Abstractions;

// Register addresses belong to the application contract; the concrete PLC client remains in Infrastructure.
public static class PlcVarsPatterns
{
    public static NviRegisters Nvi { get; } = new();
    public static NciRegisters Nci { get; } = new();
}

public sealed class NciRegisters { }

public sealed class NviRegisters
{
    public int BunkerTargetWeight(int bunker) => 1000 + bunker * 10;
    public int BunkerPriority(int bunker) => 1001 + bunker * 10;
    public int StartDosing(int bunker) => 1002 + bunker * 10;
    public int BunkerCalibWeight(int bunker) => 2000 + bunker * 10;
    public int BunkerCalibCommand(int bunker) => 2001 + bunker * 10;
}
