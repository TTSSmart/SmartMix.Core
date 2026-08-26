namespace SmartMix.Core.Contracts.BaseModels;

/// <summary>
/// DTO отчета о потреблении
/// </summary>
public record ConsumptionReportRequest
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }
    public int? LineNumber { get; init; }
}

public record ConsumptionReportResponse
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }
    public List<ConsumptionMaterialDto> Materials { get; init; } = new();
    public Dictionary<int, Dictionary<int, decimal>> BunkerBreakdown { get; init; } = new();
}

public record ConsumptionMaterialDto
{
    public int MaterialId { get; init; }
    public decimal TotalConsumedKg { get; init; }
    public DateTime LastConsumption { get; init; }
    public int LastApplicationId { get; init; }
    public int LastBatchNumber { get; init; }
}

/// <summary>
/// DTO протокола заявки
/// </summary>
public record ApplicationProtocolResponse
{
    public int ApplicationId { get; init; }
    public string WayBill { get; init; } = string.Empty;
    public string ClientName { get; init; } = string.Empty;
    public string CarNumber { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public decimal VolumeM3 { get; init; }
    public decimal FactVolumeM3 { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public bool IsCompleted { get; init; }
    public List<LayerProtocolDto> Layers { get; init; } = new();
}

public record LayerProtocolDto
{
    public int Number { get; init; }
    public string RecipeName { get; init; } = string.Empty;
    public decimal VolumeM3 { get; init; }
    public string Status { get; init; } = string.Empty;
}