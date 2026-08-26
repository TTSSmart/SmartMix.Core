using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartMix.Core.Infrastructure.Configuration;

namespace SmartMix.Core.Infrastructure.Persistence.Migrations;

public class MigrationRunner
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrationRunner> _logger;
    private readonly IOptions<DatabaseOptions> _options;
    
    public MigrationRunner(IServiceProvider serviceProvider, ILogger<MigrationRunner> logger, IOptions<DatabaseOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options;
    }
    
    public async Task RunAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Running database migrations...");
        
        using var scope = _serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        
        try
        {
            runner.MigrateUp();
            _logger.LogInformation("Migrations completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Migration failed");
            throw;
        }
    }
    
    public async Task RollbackAsync(int steps, CancellationToken ct = default)
    {
        _logger.LogWarning("Rolling back {Steps} migrations", steps);
        
        using var scope = _serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        
        for (int i = 0; i < steps; i++)
        {
            runner.MigrateDown(1);
        }
        
        _logger.LogInformation("Rollback completed");
    }
    
    public static IServiceCollection AddMigrations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddMySql5()
                .WithGlobalConnectionString(configuration.GetConnectionString("Default") ?? 
                    configuration.GetSection(DatabaseOptions.SectionName).GetValue<string>("ConnectionString"))
                .ScanIn(typeof(InitialSchema).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());
        
        return services;
    }
}