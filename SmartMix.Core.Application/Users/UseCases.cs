using SmartMix.Core.Application.Abstractions;
using SmartMix.Core.Domain.Entities;
using SmartMix.Core.Domain.ValueObjects;
using SmartMix.Core.Domain.Events;

namespace SmartMix.Core.Application.Users;

/// <summary>
/// Use Case: Аутентификация пользователя
/// </summary>
public class AuthenticateUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IDomainEventPublisher _eventPublisher;
    
    public AuthenticateUserUseCase(IUserRepository userRepository, IDomainEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _eventPublisher = eventPublisher;
    }
    
    public async Task<AuthenticateResult> ExecuteAsync(AuthenticateCommand command, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByUsernameAsync(command.Username, ct);
        if (user == null)
            return AuthenticateResult.Failure("Пользователь не найден");
        
        // Проверка пароля (в реальности: BCrypt/Argon2)
        if (!VerifyPassword(command.Password, user.PasswordHash))
            return AuthenticateResult.Failure("Неверный пароль");
        
        if (!user.IsActive)
            return AuthenticateResult.Failure("Пользователь заблокирован");
        
        // Генерация токена (JWT в реальности)
        var token = GenerateToken(user);
        
        _eventPublisher.Publish(new UserLoggedInEvent
        {
            UserId = user.Id,
            Username = user.Name,
            IpAddress = command.IpAddress
        });
        
        return AuthenticateResult.Ok(token, user);
    }
    
    private bool VerifyPassword(string password, string hash)
    {
        // Заглушка - в реальности BCrypt.Verify(password, hash)
        return password == hash;
    }
    
    private string GenerateToken(User user)
    {
        // Заглушка - в реальности JWT
        return $"token_{user.Id}_{DateTime.UtcNow.Ticks}";
    }
}

public record AuthenticateCommand(string Username, string Password, string IpAddress);
public record AuthenticateResult(bool Success, string? ErrorMessage, string? Token, User? User)
{
    public static AuthenticateResult Ok(string token, User user) => new(true, null, token, user);
    public static AuthenticateResult Failure(string error) => new(false, error, null, null);
}

/// <summary>
/// Use Case: Смена пароля
/// </summary>
public class ChangePasswordUseCase
{
    private readonly IUserRepository _userRepository;
    
    public ChangePasswordUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<ChangePasswordResult> ExecuteAsync(ChangePasswordCommand command, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, ct);
        if (user == null)
            return ChangePasswordResult.Failure("Пользователь не найден");
        
        if (!VerifyPassword(command.CurrentPassword, user.PasswordHash))
            return ChangePasswordResult.Failure("Текущий пароль неверен");
        
        user.PasswordHash = HashPassword(command.NewPassword);
        await _userRepository.UpdateAsync(user, ct);
        
        return ChangePasswordResult.Ok();
    }
    
    private bool VerifyPassword(string password, string hash) => password == hash;
    private string HashPassword(string password) => password; // Заглушка
}

public record ChangePasswordCommand(int UserId, string CurrentPassword, string NewPassword);
public record ChangePasswordResult(bool Success, string? ErrorMessage)
{
    public static ChangePasswordResult Ok() => new(true, null);
    public static ChangePasswordResult Failure(string error) => new(false, error);
}
