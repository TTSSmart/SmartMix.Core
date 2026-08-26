using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartMix.Core.Application.Abstractions;

namespace SmartMix.Core.API.HealthChecks;

public class PlcHealthCheck : IHealthCheck
{
    private readonly IPlcClient _plcClient;
    private readonly ILogger<PlcHealthCheck> _logger;
    
    public PlcHealthCheck(IPlcClient plcClient, ILogger<PlcHealthCheck> logger)
    {
        _plcClient = plcClient;
        _logger = logger;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            if (!_plcClient.IsConnected)
            {
                return HealthCheckResult.Unhealthy("PLC not connected");
            }
            
            // Try reading a test register
            var testRegisters = await _plcClient.ReadHoldingRegistersAsync(0, 1, ct);
            
            return HealthCheckResult.Healthy("PLC connection OK", new Dictionary<string, object>
            {
                ["connected"] = true,
                ["testRegistersRead"] = testRegisters.Length
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PLC health check failed");
            return HealthCheckResult.Unhealthy("PLC health check failed", ex);
        }
    }
}

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ISmartMixDatabase _database;
    private readonly ILogger<DatabaseHealthCheck> _logger;
    
    public DatabaseHealthCheck(ISmartMixDatabase database, ILogger<DatabaseHealthCheck> logger)
    {
        _database = database;
        _logger = logger;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            using var conn = await _database.GetConnectionAsync(ct);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1";
            await cmd.ExecuteScalarAsync(ct);
            
            return HealthCheckResult.Healthy("Database connection OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database health check failed", ex);
        }
    }
}

public class LicenseHealthCheck : IHealthCheck
{
    private readonly ILicenseService _licenseService;
    private readonly ILogger<LicenseHealthCheck> _logger;
    
    public LicenseHealthCheck(ILicenseService licenseService, ILogger<LicenseHealthCheck> logger)
    {
        _licenseService = licenseService;
        _logger = logger;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            var license = await _licenseService.GetCurrentLicenseAsync(ct);
            
            if (license == null)
            {
                return HealthCheckResult.Degraded("No license configured");
            }
            
            if (license.IsExpired)
            {
                return HealthCheckResult.Unhealthy($"License expired on {license.EndDate:yyyy-MM-dd}");
            }
            
            if (license.TimeRemaining < TimeSpan.FromDays(30))
            {
                return HealthCheckResult.Degraded($"License expires in {license.TimeRemaining.Days} days");
            }
            
            return HealthCheckResult.Healthy("License valid", new Dictionary<string, object>
            {
                ["licenseType"] = license.LicenseType,
                ["isPermanent"] = license.IsPermanent,
                ["daysRemaining"] = license.TimeRemaining.Days
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "License health check failed");
            return HealthCheckResult.Unhealthy("License health check failed", ex);
        }
    }
}

public static class HealthCheckExtensions
{
    public static IServiceCollection AddSmartMixHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<PlcHealthCheck>("plc", tags: new[] { "critical" })
            .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "critical" })
            .AddCheck<LicenseHealthCheck>("license", tags: new[] { "warning" });
        
        return services;
    }
    
    public static IApplicationBuilder UseSmartMixHealthChecks(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        data = e.Value.Data
                    }),
                    totalDuration = report.TotalDuration
                };
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
            }
        });
        
        app.UseHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("critical")
        });
        
        app.UseHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => false
        });
        
        return app;
    }
}