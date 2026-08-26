using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartMix.Core.Application.Abstractions;
using SmartMix.Core.Domain.Services;
using SmartMix.Core.Domain.Events;
using SmartMix.Core.Infrastructure.Configuration;

namespace SmartMix.Core.Infrastructure.HostedServices;

/// <summary>
/// Фоновый сервис опроса ПЛК
/// </summary>
public class PlcPollingHostedService : BackgroundService
{
    private readonly IPlcClient _plcClient;
    private readonly IPlcRegisterMapParser _registerParser;
    private readonly IOptions<PlcOptions> _options;
    private readonly ILogger<PlcPollingHostedService> _logger;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly BatchingOrchestrator _batchingOrchestrator;
    private readonly AlarmProcessor _alarmProcessor;
    private readonly ConsumptionTracker _consumptionTracker;
    
    private RegistersData? _lastRegisters;
    
    public PlcPollingHostedService(
        IPlcClient plcClient,
        IPlcRegisterMapParser registerParser,
        IOptions<PlcOptions> options,
        ILogger<PlcPollingHostedService> logger,
        IDomainEventPublisher eventPublisher,
        BatchingOrchestrator batchingOrchestrator,
        AlarmProcessor alarmProcessor,
        ConsumptionTracker consumptionTracker)
    {
        _plcClient = plcClient;
        _registerParser = registerParser;
        _options = options;
        _logger = logger;
        _eventPublisher = eventPublisher;
        _batchingOrchestrator = batchingOrchestrator;
        _alarmProcessor = alarmProcessor;
        _consumptionTracker = consumptionTracker;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting PLC polling service...");
        
        // Загрузить карту регистров
        var registerMap = await _registerParser.ParseAsync(_options.Value.RegisterMapCsv, stoppingToken);
        
        // Подключиться к ПЛК
        await _plcClient.ConnectAsync(stoppingToken);
        
        // Подписаться на события
        _plcClient.ConnectionChanged += OnConnectionChanged;
        _plcClient.RegistersUpdated += OnRegistersUpdated;
        
        _logger.LogInformation("PLC polling started for {Host}:{Port}", _options.Value.Host, _options.Value.Port);
        
        var pollInterval = TimeSpan.FromMilliseconds(_options.Value.PollIntervalMs);
        using var timer = new PeriodicTimer(pollInterval);
        
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await PollOnceAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("PLC polling stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PLC polling error");
        }
        finally
        {
            _plcClient.ConnectionChanged -= OnConnectionChanged;
            _plcClient.RegistersUpdated -= OnRegistersUpdated;
            await _plcClient.DisconnectAsync();
        }
    }
    
    private async Task PollOnceAsync(CancellationToken ct)
    {
        if (!_plcClient.IsConnected) return;
        
        try
        {
            var options = _options.Value;
            var count = options.EndAddress - options.StartAddress + 1;
            
            var values = await _plcClient.ReadHoldingRegistersAsync(options.StartAddress, (ushort)count, ct);
            
            var registers = new RegistersData(values, options.StartAddress);
            _lastRegisters = registers;
            
            // Обновить механизмы в активных замесах
            UpdateActiveBatches(registers);
            
            // Проверить алармы
            CheckAlarms(registers);
            
            // Обновить потребление
            UpdateConsumption(registers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during PLC poll");
        }
    }
    
    private void UpdateActiveBatches(RegistersData registers)
    {
        foreach (var context in _batchingOrchestrator.GetAllContexts().Values)
        {
            // Обновляем состояние механизмов для этого смесителя
            // В реальности: получаем механизмы из репозитория и вызываем UpdateFromRegisters
        }
    }
    
    private void CheckAlarms(RegistersData registers)
    {
        // Проверить флаги аварий в регистрах
        // В реальности: парсим регистры аварий и вызываем AlarmProcessor
    }
    
    private void UpdateConsumption(RegistersData registers)
    {
        // Отслеживать изменения весов в бункерах
        // В реальности: сравниваем с предыдущими значениями
    }
    
    private void OnConnectionChanged(bool connected)
    {
        _logger.LogWarning("PLC connection {Status}", connected ? "restored" : "lost");
        
        _eventPublisher.Publish(connected 
            ? new PlcConnectionRestoredEvent { PlcAddress = $"{_options.Value.Host}:{_options.Value.Port}", LineNumber = _options.Value.LineNumber }
            : new PlcConnectionLostEvent { PlcAddress = $"{_options.Value.Host}:{_options.Value.Port}", LineNumber = _options.Value.LineNumber });
    }
    
    private void OnRegistersUpdated(ushort[] values, ushort startAddress)
    {
        _eventPublisher.Publish(new PlcRegistersUpdatedEvent
        {
            PlcAddress = $"{_options.Value.Host}:{_options.Value.Port}",
            RegisterCount = values.Length
        });
    }
}

/// <summary>
/// Фоновый сервис автосигналов
/// </summary>
public class AutoStartSignalHostedService : BackgroundService
{
    private readonly IAutoStartSignalReader _signalReader;
    private readonly IApplicationStarter _applicationStarter;
    private readonly IOptions<LineOptions> _lineOptions;
    private readonly ILogger<AutoStartSignalHostedService> _logger;
    private readonly IDomainEventPublisher _eventPublisher;
    
    public AutoStartSignalHostedService(
        IAutoStartSignalReader signalReader,
        IApplicationStarter applicationStarter,
        IOptions<LineOptions> lineOptions,
        ILogger<AutoStartSignalHostedService> logger,
        IDomainEventPublisher eventPublisher)
    {
        _signalReader = signalReader;
        _applicationStarter = applicationStarter;
        _lineOptions = lineOptions;
        _logger = logger;
        _eventPublisher = eventPublisher;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_lineOptions.Value.EnableAutoStart)
        {
            _logger.LogInformation("AutoStart disabled for line {LineNumber}", _lineOptions.Value.LineNumber);
            return;
        }
        
        _logger.LogInformation("Starting AutoStart signal monitoring...");
        
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var signals = await _signalReader.ReadSignalsAsync(_lineOptions.Value.LineNumber, stoppingToken);
                
                foreach (var signal in signals)
                {
                    if (signal.IsActive)
                    {
                        await _applicationStarter.ProcessSignalAsync(signal, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading auto-start signals");
            }
        }
    }
}

/// <summary>
/// Фоновый сервис мониторинга алармов
/// </summary>
public class AlarmMonitoringHostedService : BackgroundService
{
    private readonly AlarmProcessor _alarmProcessor;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AlarmMonitoringHostedService> _logger;
    
    public AlarmMonitoringHostedService(
        AlarmProcessor alarmProcessor,
        INotificationService notificationService,
        ILogger<AlarmMonitoringHostedService> logger)
    {
        _alarmProcessor = alarmProcessor;
        _notificationService = notificationService;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var activeAlarms = _alarmProcessor.GetActiveAlarms();
                
                foreach (var (key, state) in activeAlarms)
                {
                    foreach (var alarm in state.ActiveAlarms.Values)
                    {
                        if (!alarm.AcknowledgedAt.HasValue && 
                            DateTime.UtcNow - alarm.RaisedAt > TimeSpan.FromMinutes(1))
                        {
                            // Эскалация неуведомленного аларма
                            await _notificationService.SendAlarmAsync(new AlarmNotification
                            {
                                MechanismType = key / 100,
                                MechanismNumber = key % 100,
                                AlarmCode = (int)alarm.Code,
                                Message = alarm.Message,
                                Timestamp = alarm.RaisedAt
                            }, stoppingToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in alarm monitoring");
            }
        }
    }
}

/// <summary>
/// Фоновый сервис записи моточасов
/// </summary>
public class MotoHourRecordingHostedService : BackgroundService
{
    private readonly IMotoHourService _motoHourService;
    private readonly IPlcClient _plcClient;
    private readonly IOptions<PlcOptions> _options;
    private readonly ILogger<MotoHourRecordingHostedService> _logger;
    
    public MotoHourRecordingHostedService(
        IMotoHourService motoHourService,
        IPlcClient plcClient,
        IOptions<PlcOptions> options,
        ILogger<MotoHourRecordingHostedService> logger)
    {
        _motoHourService = motoHourService;
        _plcClient = plcClient;
        _options = options;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                if (!_plcClient.IsConnected) continue;
                
                // Читать моточасы из ПЛК и сохранять в БД
                // В реальности: читаем регистры моточасов для каждого механизма
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording moto hours");
            }
        }
    }
}

/// <summary>
/// Фоновый сервис мониторинга лицензии
/// </summary>
public class LicenseMonitoringHostedService : BackgroundService
{
    private readonly ILicenseService _licenseService;
    private readonly IOptions<LicenseOptions> _options;
    private readonly ILogger<LicenseMonitoringHostedService> _logger;
    
    public LicenseMonitoringHostedService(
        ILicenseService licenseService,
        IOptions<LicenseOptions> options,
        ILogger<LicenseMonitoringHostedService> logger)
    {
        _licenseService = licenseService;
        _options = options;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrEmpty(_options.Value.SerialNumber))
        {
            _logger.LogWarning("No license serial number configured");
            return;
        }
        
        using var timer = new PeriodicTimer(TimeSpan.FromHours(_options.Value.CheckIntervalHours));
        
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var license = await _licenseService.GetCurrentLicenseAsync(stoppingToken);
                
                if (license?.IsExpired == true)
                {
                    _logger.LogError("License expired!");
                    // Можно остановить работу или перевести в демо-режим
                }
                else if (license?.TimeRemaining < TimeSpan.FromDays(7))
                {
                    _logger.LogWarning("License expires soon: {TimeRemaining}", license.TimeRemaining);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking license");
            }
        }
    }
}