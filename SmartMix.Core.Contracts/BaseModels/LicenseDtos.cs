namespace SmartMix.Core.Contracts.BaseModels;

/// <summary>
/// DTO лицензии
/// </summary>
public record LicenseInfoDto
{
    public int LicenseType { get; init; }
    public DateTime EndDate { get; init; }
    public string SerialNumber { get; init; } = string.Empty;
    public string HardwareId { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime? ActivatedAt { get; init; }
    public int MaxLines { get; init; }
    public int MaxMixers { get; init; }
    public bool IsPermanent { get; init; }
    public bool IsExpired { get; init; }
    public TimeSpan TimeRemaining { get; init; }
}

/// <summary>
/// DTO активации лицензии
/// </summary>
public record ActivateLicenseRequest
{
    public string SerialNumber { get; init; } = string.Empty;
}

public record ActivateLicenseResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public LicenseInfoDto? License { get; init; }
}

/// <summary>
/// DTO проверки лицензии
/// </summary>
public record ValidateLicenseRequest
{
    public string SerialNumber { get; init; } = string.Empty;
}

public record ValidateLicenseResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public LicenseInfoDto? License { get; init; }
    public bool IsExpired { get; init; }
    public TimeSpan TimeRemaining { get; init; }
}