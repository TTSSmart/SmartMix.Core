using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartMix.Core.Application.Abstractions;
using SmartMix.Core.Domain.Entities.Licenses;
using SmartMix.Core.Infrastructure.Configuration;

namespace SmartMix.Core.Infrastructure.Services;

public class LicenseService : ILicenseService
{
    private readonly IOptions<LicenseOptions> _options;
    private readonly ILogger<LicenseService> _logger;
    private LicenseInfo? _currentLicense;
    
    public LicenseService(IOptions<LicenseOptions> options, ILogger<LicenseService> logger)
    {
        _options = options;
        _logger = logger;
    }
    
    public async Task<LicenseValidationResult> ValidateAsync(string serialNumber, string hardwareId, CancellationToken ct = default)
    {
        // В реальности: проверка через криптографию, запрос к серверу лицензий
        // Заглушка:
        var license = new LicenseInfo
        {
            SerialNumber = serialNumber,
            HardwareId = hardwareId,
            LicenseType = 1, // Постоянная
            EndDate = DateTime.Now.AddYears(10),
            IsActive = true,
            ActivatedAt = DateTime.Now
        };
        
        _currentLicense = license;
        return new LicenseValidationResult(true, license, null);
    }
    
    public async Task<LicenseInfo?> GetCurrentLicenseAsync(CancellationToken ct = default)
    {
        if (_currentLicense != null) return _currentLicense;
        
        if (!string.IsNullOrEmpty(_options.Value.SerialNumber))
        {
            return await ValidateAsync(_options.Value.SerialNumber, GetHardwareId(), ct).ContinueWith(t => t.Result.License, ct);
        }
        
        return null;
    }
    
    public async Task<bool> ActivateAsync(string serialNumber, CancellationToken ct = default)
    {
        var result = await ValidateAsync(serialNumber, GetHardwareId(), ct);
        return result.IsValid;
    }
    
    public async Task DeactivateAsync(CancellationToken ct = default)
    {
        _currentLicense = null;
        await Task.CompletedTask;
    }
    
    private string GetHardwareId()
    {
        // В реальности: чтение уникального ID железа
        return Environment.MachineName;
    }
}

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    
    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }
    
    public async Task SendAlarmAsync(AlarmNotification notification, CancellationToken ct = default)
    {
        _logger.LogWarning("ALARM: Mechanism {Type}#{Number} Code {Code}: {Message}",
            notification.MechanismType, notification.MechanismNumber, notification.AlarmCode, notification.Message);
        
        // В реальности: отправка в Telegram, Email, SMS, SignalR
        await Task.CompletedTask;
    }
    
    public async Task SendInfoAsync(InfoNotification notification, CancellationToken ct = default)
    {
        _logger.LogInformation("INFO: {Title} - {Message}", notification.Title, notification.Message);
        await Task.CompletedTask;
    }
    
    public async Task SendTelegramAsync(string message, CancellationToken ct = default)
    {
        _logger.LogInformation("TELEGRAM: {Message}", message);
        // В реальности: отправка через Bot API
        await Task.CompletedTask;
    }
}

public class SettingsService : ISettingsService
{
    private readonly ISmartMixDatabase _database;
    private readonly ILogger<SettingsService> _logger;
    private readonly Dictionary<string, object> _cache = new();
    
    public SettingsService(ISmartMixDatabase database, ILogger<SettingsService> logger)
    {
        _database = database;
        _logger = logger;
    }
    
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        if (_cache.TryGetValue(key, out var cached))
            return cached as T;
        
        const string sql = "SELECT setting_value FROM settings WHERE setting_key = @Key AND line_number = @LineNumber";
        using var conn = _database.GetConnection();
        var value = await conn.QueryFirstOrDefaultAsync<string>(sql, new { Key = key, LineNumber = 1 });
        
        if (value != null)
        {
            var result = System.Text.Json.JsonSerializer.Deserialize<T>(value);
            _cache[key] = result!;
            return result;
        }
        
        return null;
    }
    
    public async Task SetAsync<T>(string key, T value, CancellationToken ct = default) where T : class
    {
        var json = System.Text.Json.JsonSerializer.Serialize(value);
        
        const string sql = @"
            INSERT INTO settings (setting_key, setting_value, line_number, updated_at)
            VALUES (@Key, @Value, @LineNumber, @UpdatedAt)
            ON DUPLICATE KEY UPDATE setting_value = @Value, updated_at = @UpdatedAt";
        
        using var conn = _database.GetConnection();
        await conn.ExecuteAsync(sql, new { Key = key, Value = json, LineNumber = 1, UpdatedAt = DateTime.UtcNow });
        
        _cache[key] = value;
    }
    
    public async Task<LineConfiguration> GetLineConfigurationAsync(int lineNumber, CancellationToken ct = default)
    {
        // Загрузка полной конфигурации линии из БД
        return new LineConfiguration(lineNumber, $"Line {lineNumber}", 
            new PlcEndpoint("192.168.1.100", 502), 
            new DatabaseConfig("", 100), new());
    }
}

public class MotoHourService : IMotoHourService
{
    private readonly ISmartMixDatabase _database;
    private readonly ILogger<MotoHourService> _logger;
    
    public MotoHourService(ISmartMixDatabase database, ILogger<MotoHourService> logger)
    {
        _database = database;
        _logger = logger;
    }
    
    public async Task RecordMotoHoursAsync(int mechanismType, int mechanismNumber, TimeSpan duration, CancellationToken ct = default)
    {
        const string sql = @"
            INSERT INTO moto_hours (mechanism_type, mechanism_number, hours, recorded_at)
            VALUES (@Type, @Number, @Hours, @RecordedAt)
            ON DUPLICATE KEY UPDATE hours = hours + @Hours, recorded_at = @RecordedAt";
        
        using var conn = _database.GetConnection();
        await conn.ExecuteAsync(sql, new
        {
            Type = mechanismType,
            Number = mechanismNumber,
            Hours = duration.TotalHours,
            RecordedAt = DateTime.UtcNow
        });
    }
    
    public async Task<MotoHoursReport> GetMotoHoursReportAsync(int? mechanismType = null, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var sql = "SELECT mechanism_type, mechanism_number, SUM(hours) as total_hours FROM moto_hours WHERE 1=1";
        var parameters = new DynamicParameters();
        
        if (mechanismType.HasValue)
        {
            sql += " AND mechanism_type = @Type";
            parameters.Add("Type", mechanismType.Value);
        }
        
        if (from.HasValue)
        {
            sql += " AND recorded_at >= @From";
            parameters.Add("From", from.Value);
        }
        
        if (to.HasValue)
        {
            sql += " AND recorded_at <= @To";
            parameters.Add("To", to.Value);
        }
        
        sql += " GROUP BY mechanism_type, mechanism_number";
        
        using var conn = _database.GetConnection();
        var rows = await conn.QueryAsync(sql, parameters);
        
        var report = new Dictionary<int, Dictionary<int, TimeSpan>>();
        
        foreach (var row in rows)
        {
            var type = (int)row.mechanism_type;
            var number = (int)row.mechanism_number;
            var hours = (double)row.total_hours;
            
            if (!report.ContainsKey(type))
                report[type] = new Dictionary<int, TimeSpan>();
            
            report[type][number] = TimeSpan.FromHours(hours);
        }
        
        return new MotoHoursReport(report);
    }
}