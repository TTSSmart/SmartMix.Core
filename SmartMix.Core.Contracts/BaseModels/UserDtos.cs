namespace SmartMix.Core.Contracts.BaseModels;

/// <summary>
/// DTO аутентификации
/// </summary>
public record AuthenticateRequest
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record AuthenticateResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? Token { get; init; }
    public UserDto? User { get; init; }
}

public record UserDto
{
    public int Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public List<string> Permissions { get; init; } = new();
}

/// <summary>
/// DTO смены пароля
/// </summary>
public record ChangePasswordRequest
{
    public int UserId { get; init; }
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public record ChangePasswordResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}