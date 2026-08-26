using SmartMix.Core.Application.Abstractions;
using SmartMix.Core.Domain.Entities;
using SmartMix.Core.Domain.ValueObjects;
using SmartMix.Core.Domain.Events;

namespace SmartMix.Core.Application.Reports;

/// <summary>
/// Use Case: Генерация отчёта о потреблении
/// </summary>
public class GenerateConsumptionReportUseCase
{
    private readonly IReportRepository _reportRepository;
    
    public GenerateConsumptionReportUseCase(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }
    
    public async Task<ConsumptionReportDto> ExecuteAsync(GenerateConsumptionReportCommand command, CancellationToken ct = default)
    {
        var report = await _reportRepository.GetConsumptionReportAsync(
            command.From, command.To, command.LineNumber, ct);
        
        return new ConsumptionReportDto
        {
            From = report.From,
            To = report.To,
            Materials = report.Materials.Select(m => new ConsumptionMaterialDto
            {
                MaterialId = m.MaterialId.Value,
                TotalConsumedKg = m.TotalConsumed.Kilograms,
                LastConsumption = m.LastConsumption,
                LastApplicationId = m.LastApplicationId,
                LastBatchNumber = m.LastBatchNumber
            }).ToList(),
            BunkerBreakdown = report.BunkerBreakdown.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToDictionary(
                    m => m.Key.Value,
                    m => m.Value.Kilograms))
        };
    }
}

public record GenerateConsumptionReportCommand(DateTime From, DateTime To, int? LineNumber = null);

public record ConsumptionReportDto
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
/// Use Case: Генерация протокола заявки
/// </summary>
public class GenerateApplicationProtocolUseCase
{
    private readonly IReportRepository _reportRepository;
    private readonly IApplicationRepository _applicationRepository;
    
    public GenerateApplicationProtocolUseCase(
        IReportRepository reportRepository,
        IApplicationRepository applicationRepository)
    {
        _reportRepository = reportRepository;
        _applicationRepository = applicationRepository;
    }
    
    public async Task<ApplicationProtocolDto> ExecuteAsync(int applicationId, CancellationToken ct = default)
    {
        var application = await _applicationRepository.GetByIdWithLayersAsync(applicationId, ct);
        if (application == null)
            throw new ArgumentException($"Заявка {applicationId} не найдена");
        
        var report = await _reportRepository.GetApplicationReportAsync(applicationId, ct);
        
        return new ApplicationProtocolDto
        {
            ApplicationId = application.Id,
            WayBill = application.WayBill,
            ClientName = application.Client?.Name ?? string.Empty,
            CarNumber = application.Car?.Number ?? string.Empty,
            ProductName = application.Product?.Name ?? string.Empty,
            Volume = application.Volume.CubicMeters,
            FactVolume = application.FactVolume.CubicMeters,
            StartTime = application.StartTime,
            EndTime = application.EndTime,
            IsCompleted = application.IsCompleted,
            Layers = application.Layers.Select(l => new LayerProtocolDto
            {
                Number = l.Number,
                RecipeName = l.Recipe?.Name ?? string.Empty,
                Volume = l.Volume.CubicMeters,
                Status = l.Status.ToString()
            }).ToList()
        };
    }
}

public record ApplicationProtocolDto
{
    public int ApplicationId { get; init; }
    public string WayBill { get; init; } = string.Empty;
    public string ClientName { get; init; } = string.Empty;
    public string CarNumber { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public decimal Volume { get; init; }
    public decimal FactVolume { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public bool IsCompleted { get; init; }
    public List<LayerProtocolDto> Layers { get; init; } = new();
}

public record LayerProtocolDto
{
    public int Number { get; init; }
    public string RecipeName { get; init; } = string.Empty;
    public decimal Volume { get; init; }
    public string Status { get; init; } = string.Empty;
}