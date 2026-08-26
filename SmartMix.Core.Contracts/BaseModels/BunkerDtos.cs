namespace SmartMix.Core.Contracts.BaseModels;

/// <summary>
/// DTO бункера
/// </summary>
public record BunkerDto
{
    public int Number { get; init; }
    public int LineNumber { get; init; }
    public bool IsOn { get; init; }
    public int ComponentId { get; init; }
    public string ComponentName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public decimal CurrentWeightKg { get; init; }
    public bool IsActive { get; init; }
    public bool IsInAlarm { get; init; }
    public string AlarmMessage { get; init; } = string.Empty;
    public decimal ConsumptionWeightKg { get; init; }
    public int AutoLoadCount { get; init; }
    public int ManualLoadCount { get; init; }
    public bool HighLevel { get; init; }
    public bool LowLevel { get; init; }
    public bool FilterActive { get; init; }
}

/// <summary>
/// DTO установки компонента в бункер
/// </summary>
public record SetBunkerComponentRequest
{
    public int BunkerNumber { get; init; }
    public int LineNumber { get; init; }
    public int MaterialId { get; init; }
    public int UserId { get; init; }
}

public record SetBunkerComponentResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public BunkerDto? Bunker { get; init; }
}

/// <summary>
/// DTO переключения бункера
/// </summary>
public record ToggleBunkerRequest
{
    public int BunkerNumber { get; init; }
    public int LineNumber { get; init; }
    public bool IsOn { get; init; }
    public int UserId { get; init; }
}

public record ToggleBunkerResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public BunkerDto? Bunker { get; init; }
}

/// <summary>
/// DTO калибровки бункера
/// </summary>
public record CalibrateBunkerRequest
{
    public int BunkerNumber { get; init; }
    public int LineNumber { get; init; }
    public decimal ReferenceWeightKg { get; init; }
}

public record CalibrateBunkerResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}