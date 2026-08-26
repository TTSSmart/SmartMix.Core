using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartMix.Core.Application.Abstractions;
using SmartMix.Core.Application.Batching;
using SmartMix.Core.Application.Recipes;
using SmartMix.Core.Application.Bunkers;
using SmartMix.Core.Application.Reports;
using SmartMix.Core.Application.Users;
using SmartMix.Core.Application.Licenses;
using SmartMix.Core.Application.Common;
using SmartMix.Core.Domain.Services;
using SmartMix.Core.Domain.Events;
using SmartMix.Core.Infrastructure.Persistence;
using SmartMix.Core.Infrastructure.Plc;
using SmartMix.Core.Infrastructure.Logging;
using SmartMix.Core.Infrastructure.Configuration;
using Serilog;
using SmartMix.Core.Infrastructure.Persistence.Repositories;
using SmartMix.Core.Infrastructure.Services;
using SmartMix.Core.Infrastructure.HostedServices;

namespace SmartMix.Core;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        // Configuration
        builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
        
        // Serilog
        builder.Services.AddSerilog((services, config) =>
        {
            config.ReadFrom.Configuration(builder.Configuration)
                  .ReadFrom.Services(services)
                  .Enrich.FromLogContext();
        });
        
        // Options
        builder.Services.Configure<PlcOptions>(builder.Configuration.GetSection(PlcOptions.SectionName));
        builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));
        builder.Services.Configure<LoggingOptions>(builder.Configuration.GetSection(LoggingOptions.SectionName));
        builder.Services.Configure<LineOptions>(builder.Configuration.GetSection(LineOptions.SectionName));
        builder.Services.ValidateOptions<PlcOptions>();
        builder.Services.ValidateOptions<DatabaseOptions>();
        
        // Core Domain Services
        builder.Services.AddSingleton<BatchingOrchestrator>();
        builder.Services.AddSingleton<RecipeCalculator>();
        builder.Services.AddSingleton<ConsumptionTracker>();
        builder.Services.AddSingleton<AlarmProcessor>();
        builder.Services.AddSingleton<IDomainEventPublisher, InMemoryDomainEventPublisher>();
        
        // Infrastructure
        builder.Services.AddSingleton<IPlcClient, ModbusTcpClientWrapper>();
        builder.Services.AddSingleton<IPlcRegisterMapParser, CsvRegisterMapParser>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Repositories
        builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
        builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
        builder.Services.AddScoped<IBunkerRepository, BunkerRepository>();
        builder.Services.AddScoped<IComponentRepository, ComponentRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IReportRepository, ReportRepository>();
        
        // Services
        builder.Services.AddScoped<ILicenseService, LicenseService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddScoped<ISettingsService, SettingsService>();
        builder.Services.AddScoped<IMotoHourService, MotoHourService>();
        
        // Use Cases - Batching
        builder.Services.AddScoped<StartBatchUseCase>();
        builder.Services.AddScoped<DoseMaterialUseCase>();
        builder.Services.AddScoped<CompleteBatchUseCase>();
        builder.Services.AddScoped<GetBatchStatusUseCase>();
        
        // Use Cases - Recipes
        builder.Services.AddScoped<CreateRecipeUseCase>();
        builder.Services.AddScoped<UpdateRecipeUseCase>();
        builder.Services.AddScoped<CalculateRecipeMaterialsUseCase>();
        
        // Use Cases - Bunkers
        builder.Services.AddScoped<SetBunkerComponentUseCase>();
        builder.Services.AddScoped<ToggleBunkerUseCase>();
        builder.Services.AddScoped<CalibrateBunkerUseCase>();
        
        // Use Cases - Reports
        builder.Services.AddScoped<GenerateConsumptionReportUseCase>();
        builder.Services.AddScoped<GenerateApplicationProtocolUseCase>();
        
        // Use Cases - Users
        builder.Services.AddScoped<AuthenticateUserUseCase>();
        builder.Services.AddScoped<ChangePasswordUseCase>();
        
        // Use Cases - Licenses
        builder.Services.AddScoped<ActivateLicenseUseCase>();
        builder.Services.AddScoped<ValidateLicenseUseCase>();
        
        // Hosted Services
        builder.Services.AddHostedService<PlcPollingHostedService>();
        builder.Services.AddHostedService<AutoStartSignalHostedService>();
        builder.Services.AddHostedService<AlarmMonitoringHostedService>();
        builder.Services.AddHostedService<MotoHourRecordingHostedService>();
        builder.Services.AddHostedService<LicenseMonitoringHostedService>();
        
        // Build and run
        var host = builder.Build();
        
        // Initialize
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("SmartMix.Core starting...");
        logger.LogInformation("Environment: {Environment}", builder.Environment.EnvironmentName);
        
        await host.RunAsync();
    }
}