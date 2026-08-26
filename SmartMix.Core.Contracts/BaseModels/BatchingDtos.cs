namespace SmartMix.Core.Contracts.BaseModels;

/// <summary>
/// DTO для запуска замеса
/// </summary>
public record StartBatchRequest
{
    public int ApplicationId { get; init; }
    public int MixerNumber { get; init; }
}

public record StartBatchResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public int BatchId { get; init; }
    public BatchStatusDto? Status { get; init; }
}

/// <summary>
/// DTO для дозирования материала
/// </summary>
public record DoseMaterialRequest
{
    public int MixerNumber { get; init; }
    public int MaterialId { get; init; }
    public DosingMode Mode { get; init; } = DosingMode.Auto;
}

public record DoseMaterialResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public PlcDosingCommandDto? Command { get; init; }
}

public record PlcDosingCommandDto
{
    public int MixerNumber { get; init; }
    public int BunkerNumber { get; init; }
    public decimal TargetWeightKg { get; init; }
    public DosingMode Mode { get; init; }
}

/// <summary>
/// DTO статуса замеса
/// </summary>
public record BatchStatusDto
{
    public int MixerNumber { get; init; }
    public int ApplicationId { get; init; }
    public string RecipeName { get; init; } = string.Empty;
    public string Stage { get; init; } = string.Empty;
    public int CurrentMaterialId { get; init; }
    public List<int> CompletedMaterialIds { get; init; } = new();
    public int TotalMaterials { get; init; }
    public double ProgressPercent { get; init; }
    public DateTime StartedAt { get; init; }
    public TimeSpan Elapsed { get; init; }
    public TimeSpan? EstimatedRemaining { get; init; }
}

/// <summary>
/// DTO завершения замеса
/// </summary>
public record CompleteBatchRequest
{
    public int MixerNumber { get; init; }
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}

public record CompleteBatchResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public int ApplicationId { get; init; }
}

public enum DosingMode
{
    Auto = 0,
    Manual = 1,
    Fast = 2,
    Precise = 3
}