using SmartMix.Core.Application.Abstractions;
using SmartMix.Core.Domain.Entities.Licenses;
using SmartMix.Core.Domain.Events;

namespace SmartMix.Core.Application.Licenses;

/// <summary>
/// Use Case: Активация лицензии
/// </summary>
public class ActivateLicenseUseCase
{
    private readonly ILicenseService _licenseService;
    private readonly IDomainEventPublisher _eventPublisher;
    
    public ActivateLicenseUseCase(ILicenseService licenseService, IDomainEventPublisher eventPublisher)
    {
        _licenseService = licenseService;
        _eventPublisher = eventPublisher;
    }
    
    public async Task<ActivateLicenseResult> ExecuteAsync(ActivateLicenseCommand command, CancellationToken ct = default)
    {
        var result = await _licenseService.ActivateAsync(command.SerialNumber, ct);
        if (!result)
            return ActivateLicenseResult.Failure("Не удалось активировать лицензию");
        
        var license = await _licenseService.GetCurrentLicenseAsync(ct);
        
        return ActivateLicenseResult.Ok(license);
    }
}

public record ActivateLicenseCommand(string SerialNumber);
public record ActivateLicenseResult(bool Success, string? ErrorMessage, LicenseInfo? License)
{
    public static ActivateLicenseResult Ok(LicenseInfo? license) => new(true, null, license);
    public static ActivateLicenseResult Failure(string error) => new(false, error, null);
}

/// <summary>
/// Use Case: Проверка лицензии
/// </summary>
public class ValidateLicenseUseCase
{
    private readonly ILicenseService _licenseService;
    
    public ValidateLicenseUseCase(ILicenseService licenseService)
    {
        _licenseService = licenseService;
    }
    
    public async Task<ValidateLicenseResult> ExecuteAsync(ValidateLicenseCommand command, CancellationToken ct = default)
    {
        var hardwareId = GetHardwareId();
        var result = await _licenseService.ValidateAsync(command.SerialNumber, hardwareId, ct);
        
        return new ValidateLicenseResult
        {
            Success = result.IsValid,
            ErrorMessage = result.ErrorMessage,
            License = result.License,
            IsExpired = result.License?.IsExpired ?? true,
            TimeRemaining = result.License?.TimeRemaining ?? TimeSpan.Zero
        };
    }
    
    private string GetHardwareId()
    {
        // В реальности: чтение ID материнской платы, CPU, MAC адреса
        return Environment.MachineName;
    }
}

public record ValidateLicenseCommand(string SerialNumber);
public record ValidateLicenseResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public LicenseInfo? License { get; init; }
    public bool IsExpired { get; init; }
    public TimeSpan TimeRemaining { get; init; }
}
