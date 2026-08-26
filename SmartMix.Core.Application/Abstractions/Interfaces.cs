using SmartMix.Core.Domain.Entities;
using SmartMix.Core.Domain.Entities.Licenses;
using SmartMix.Core.Domain.ValueObjects;
using ApplicationEntity = SmartMix.Core.Domain.Entities.Application;

namespace SmartMix.Core.Application.Abstractions;

/// <summary>
/// Репозиторий заявок
/// </summary>
public interface IApplicationRepository
{
    Task<ApplicationEntity?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ApplicationEntity?> GetByIdWithLayersAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<ApplicationEntity>> GetActiveAsync(int lineNumber, CancellationToken ct = default);
    Task<IReadOnlyList<ApplicationEntity>> GetByStatusAsync(ApplicationStatus status, int lineNumber, CancellationToken ct = default);
    Task<int> CreateAsync(ApplicationEntity application, CancellationToken ct = default);
    Task UpdateAsync(ApplicationEntity application, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<ApplicationEntity?> GetNextInQueueAsync(int mixerNumber, CancellationToken ct = default);
}

/// <summary>
/// Репозиторий рецептов
/// </summary>
public interface IRecipeRepository
{
    Task<Recipe?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Recipe>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Recipe>> GetByCategoryAsync(int categoryId, CancellationToken ct = default);
    Task<int> CreateAsync(Recipe recipe, CancellationToken ct = default);
    Task UpdateAsync(Recipe recipe, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}

/// <summary>
/// Репозиторий бункеров
/// </summary>
public interface IBunkerRepository
{
    Task<Bunker?> GetByNumberAsync(int bunkerNumber, int lineNumber, CancellationToken ct = default);
    Task<IReadOnlyList<Bunker>> GetAllAsync(int lineNumber, CancellationToken ct = default);
    Task UpdateAsync(Bunker bunker, CancellationToken ct = default);
    Task<Bunker?> GetByMaterialAsync(int materialId, int lineNumber, CancellationToken ct = default);
}

/// <summary>
/// Репозиторий компонентов/материалов
/// </summary>
public interface IComponentRepository
{
    Task<Component?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Component>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Component>> GetByTypeAsync(ComponentType type, CancellationToken ct = default);
}

/// <summary>
/// Репозиторий пользователей
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default);
    Task<int> CreateAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
}

/// <summary>
/// Репозиторий отчётов
/// </summary>
public interface IReportRepository
{
    Task<ConsumptionReport> GetConsumptionReportAsync(DateTime from, DateTime to, int? lineNumber = null, CancellationToken ct = default);
    Task<ApplicationReport> GetApplicationReportAsync(int applicationId, CancellationToken ct = default);
    Task<BunkerConsumptionReport> GetBunkerConsumptionReportAsync(int bunkerNumber, DateTime from, DateTime to, CancellationToken ct = default);
}

/// <summary>
/// Клиент ПЛК
/// </summary>
public interface IPlcClient
{
    Task ConnectAsync(CancellationToken ct = default);
    Task DisconnectAsync();
    Task<ushort[]> ReadHoldingRegistersAsync(ushort startAddress, ushort count, CancellationToken ct = default);
    Task WriteSingleRegisterAsync(ushort address, ushort value, CancellationToken ct = default);
    Task WriteMultipleRegistersAsync(ushort startAddress, ushort[] values, CancellationToken ct = default);
    bool IsConnected { get; }
    event Action<bool> ConnectionChanged;
    event Action<ushort[], ushort> RegistersUpdated; // values, startAddress
}

/// <summary>
/// Сервис уведомлений
/// </summary>
public interface INotificationService
{
    Task SendAlarmAsync(AlarmNotification notification, CancellationToken ct = default);
    Task SendInfoAsync(InfoNotification notification, CancellationToken ct = default);
    Task SendTelegramAsync(string message, CancellationToken ct = default);
}

public record AlarmNotification(int MechanismType, int MechanismNumber, int AlarmCode, string Message, DateTime Timestamp);
public record InfoNotification(string Title, string Message, DateTime Timestamp);

/// <summary>
/// Сервис лицензий
/// </summary>
public interface ILicenseService
{
    Task<LicenseValidationResult> ValidateAsync(string serialNumber, string hardwareId, CancellationToken ct = default);
    Task<LicenseInfo?> GetCurrentLicenseAsync(CancellationToken ct = default);
    Task<bool> ActivateAsync(string serialNumber, CancellationToken ct = default);
    Task DeactivateAsync(CancellationToken ct = default);
}

public record LicenseValidationResult(bool IsValid, LicenseInfo? License, string? ErrorMessage);

/// <summary>
/// Сервис настроек
/// </summary>
public interface ISettingsService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;
    Task SetAsync<T>(string key, T value, CancellationToken ct = default) where T : class;
    Task<LineConfiguration> GetLineConfigurationAsync(int lineNumber, CancellationToken ct = default);
}

/// <summary>
/// Сервис моточасов
/// </summary>
public interface IMotoHourService
{
    Task RecordMotoHoursAsync(int mechanismType, int mechanismNumber, TimeSpan duration, CancellationToken ct = default);
    Task<MotoHoursReport> GetMotoHoursReportAsync(int? mechanismType = null, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
}

public record LineConfiguration(int LineNumber, string Name, PlcEndpoint PlcEndpoint, DatabaseConfig Database, Dictionary<string, object> Settings);
public record DatabaseConfig(string ConnectionString, int MaxPoolSize);
public record MotoHoursReport(Dictionary<int, Dictionary<int, TimeSpan>> MechanismHours);

/// <summary>
/// Unit of Work для транзакций
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
