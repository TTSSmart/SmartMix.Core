using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartMix.Core.Application.Abstractions;
using SmartMix.Core.Domain.ValueObjects;
using SmartMix.Core.Infrastructure.Configuration;
using System.Collections.Concurrent;

namespace SmartMix.Core.Infrastructure.Plc;

/// <summary>
/// Единый клиент ПЛК - объединяет функционал PlcVariables и Variables
/// Поддерживает: типизированные переменные, подписку на изменения, мок-режим, флаш записи
/// </summary>
public sealed class UnifiedPlcClient : IPlcClient, IAsyncDisposable
{
    private readonly ModbusTcpClient _modbusClient;
    private readonly IPlcRegisterMapParser _registerParser;
    private readonly IOptions<PlcOptions> _options;
    private readonly ILogger<UnifiedPlcClient> _logger;
    
    private readonly ConcurrentDictionary<string, PlcVariable> _variables = new();
    private readonly ConcurrentDictionary<string, List<Action<object>>> _subscriptions = new();
    
    private RegisterMap? _registerMap;
    private bool _isPolling;
    private CancellationTokenSource? _pollingCts;
    private ushort[]? _lastValues;
    private readonly object _pollLock = new();
    
    public event Action<bool>? ConnectionChanged;
    public event Action<ushort[], ushort>? RegistersUpdated;
    
    public bool IsConnected => _modbusClient.Connected;
    public PlcEndpoint Endpoint => new(_options.Value.Host, _options.Value.Port, _options.Value.UnitId);
    
    public UnifiedPlcClient(
        ModbusTcpClient modbusClient,
        IPlcRegisterMapParser registerParser,
        IOptions<PlcOptions> options,
        ILogger<UnifiedPlcClient> logger)
    {
        _modbusClient = modbusClient;
        _registerParser = registerParser;
        _options = options;
        _logger = logger;
        
        _modbusClient.OnException += (s, e) => _logger.LogError(e.Exception, "Modbus exception");
        _modbusClient.OnResponse += (s, e) => _logger.LogDebug("Modbus response: Function={Function}, DataLength={Length}", e.Function, e.Data.Length);
    }
    
    /// <summary>
    /// Инициализация: загрузка карты регистров, подключение к ПЛК
    /// </summary>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Initializing PLC client for {Host}:{Port}", _options.Value.Host, _options.Value.Port);
        
        // Загрузить карту регистров
        _registerMap = await _registerParser.ParseAsync(_options.Value.RegisterMapCsv, ct);
        _logger.LogInformation("Register map loaded: {Count} registers, range {Start}-{End}", 
            _registerMap.Registers.Count, _registerMap.StartAddress, _registerMap.EndAddress);
        
        // Создать переменные
        CreateVariables();
        
        // Подключиться
        await ConnectAsync(ct);
        
        // Запустить опрос
        StartPolling();
    }
    
    private void CreateVariables()
    {
        foreach (var (name, info) in _registerMap!.Registers)
        {
            var variable = CreateVariable(name, info);
            _variables.TryAdd(name, variable);
        }
    }
    
    private PlcVariable CreateVariable(string name, RegisterInfo info)
    {
        return info.Type switch
        {
            VariableType.Bool => new BoolVariable(name, info.Address, info.BitMask, info.AccessLevel, info.Description),
            VariableType.Int => new IntVariable(name, info.Address, info.AccessLevel, info.Description),
            VariableType.Uint => new UIntVariable(name, info.Address, info.AccessLevel, info.Description),
            VariableType.Float => new FloatVariable(name, info.Address, info.AccessLevel, info.Description),
            VariableType.Array => new ArrayVariable(name, info.Address, info.ArraySize, info.AccessLevel, info.Description),
            _ => new IntVariable(name, info.Address, info.AccessLevel, info.Description)
        };
    }
    
    public async Task ConnectAsync(CancellationToken ct = default)
    {
        var connected = await Task.Run(() => _modbusClient.TryConnect(), ct);
        
        if (!connected)
        {
            _logger.LogError("Failed to connect to PLC {Host}:{Port}", _options.Value.Host, _options.Value.Port);
            throw new InvalidOperationException($"Failed to connect to PLC {_options.Value.Host}:{_options.Value.Port}");
        }
        
        _logger.LogInformation("Connected to PLC {Host}:{Port}", _options.Value.Host, _options.Value.Port);
        ConnectionChanged?.Invoke(true);
    }
    
    public async Task DisconnectAsync()
    {
        StopPolling();
        await Task.Run(() => _modbusClient.Disconnect());
        _logger.LogInformation("Disconnected from PLC");
        ConnectionChanged?.Invoke(false);
    }
    
    private void StartPolling()
    {
        if (_isPolling) return;
        
        _isPolling = true;
        _pollingCts = new CancellationTokenSource();
        
        _ = Task.Run(async () =>
        {
            var interval = TimeSpan.FromMilliseconds(_options.Value.PollIntervalMs);
            var options = _options.Value;
            
            while (!_pollingCts.Token.IsCancellationRequested)
            {
                try
                {
                    await PollOnceAsync(_pollingCts.Token);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during PLC poll");
                }
                
                try
                {
                    await Task.Delay(interval, _pollingCts.Token);
                }
                catch (OperationCanceledException) { break; }
            }
            
            _isPolling = false;
        }, _pollingCts.Token);
        
        _logger.LogInformation("PLC polling started with interval {Interval}ms", _options.Value.PollIntervalMs);
    }
    
    private void StopPolling()
    {
        _pollingCts?.Cancel();
        _pollingCts?.Dispose();
        _pollingCts = null;
        _logger.LogInformation("PLC polling stopped");
    }
    
    private async Task PollOnceAsync(CancellationToken ct)
    {
        if (!_modbusClient.Connected) return;
        
        lock (_pollLock)
        {
            try
            {
                var options = _options.Value;
                var count = (ushort)(options.EndAddress - options.StartAddress + 1);
                
                var values = await _modbusClient.ReadHoldingRegistersAsync(options.StartAddress, count, ct);
                
                // Обновить переменные
                UpdateVariables(values, options.StartAddress);
                
                // Детекция изменений
                DetectChanges(values, options.StartAddress);
                
                _lastValues = values;
                RegistersUpdated?.Invoke(values, options.StartAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Poll cycle failed");
            }
        }
    }
    
    private void UpdateVariables(ushort[] values, ushort startAddress)
    {
        foreach (var (name, variable) in _variables)
        {
            if (_registerMap!.Registers.TryGetValue(name, out var info))
            {
                var offset = info.Address - startAddress;
                var size = GetVariableSize(info.Type);
                
                if (offset >= 0 && offset + size <= values.Length)
                {
                    var varValues = new ushort[size];
                    Array.Copy(values, offset, varValues, 0, size);
                    variable.UpdateFromRaw(varValues);
                }
            }
        }
    }
    
    private void DetectChanges(ushort[] values, ushort startAddress)
    {
        if (_lastValues == null) return;
        
        foreach (var (name, variable) in _variables)
        {
            if (_registerMap!.Registers.TryGetValue(name, out var info))
            {
                var offset = info.Address - startAddress;
                var size = GetVariableSize(info.Type);
                
                if (offset >= 0 && offset + size <= values.Length && offset + size <= _lastValues.Length)
                {
                    var changed = false;
                    for (int i = 0; i < size; i++)
                    {
                        if (values[offset + i] != _lastValues[offset + i])
                        {
                            changed = true;
                            break;
                        }
                    }
                    
                    if (changed)
                    {
                        NotifySubscribers(name, variable.GetValue());
                    }
                }
            }
        }
    }
    
    private void NotifySubscribers(string name, object value)
    {
        if (_subscriptions.TryGetValue(name, out var handlers))
        {
            foreach (var handler in handlers)
            {
                try { handler(value); }
                catch (Exception ex) { _logger.LogError(ex, "Error notifying subscriber for {Name}", name); }
            }
        }
    }
    
    private int GetVariableSize(VariableType type) => type switch
    {
        VariableType.Float => 2,
        VariableType.Array => 1, // будет переопределено в ArrayVariable
        _ => 1
    };
    
    // === IPlcClient Implementation ===
    
    public async Task<ushort[]> ReadHoldingRegistersAsync(ushort startAddress, ushort count, CancellationToken ct = default)
    {
        return await _modbusClient.ReadHoldingRegistersAsync(startAddress, count, ct);
    }
    
    public async Task WriteSingleRegisterAsync(ushort address, ushort value, CancellationToken ct = default)
    {
        await _modbusClient.WriteSingleRegisterAsync(address, value, ct);
    }
    
    public async Task WriteMultipleRegistersAsync(ushort startAddress, ushort[] values, CancellationToken ct = default)
    {
        await _modbusClient.WriteMultipleRegistersAsync(startAddress, values, ct);
    }
    
    // === Typed Variable Access ===
    
    public T? GetVariable<T>(string name) where T : PlcVariable
    {
        if (_variables.TryGetValue(name, out var variable) && variable is T typed)
            return typed;
        return null;
    }
    
    public BoolVariable? GetBool(string name) => GetVariable<BoolVariable>(name);
    public IntVariable? GetInt(string name) => GetVariable<IntVariable>(name);
    public UIntVariable? GetUInt(string name) => GetVariable<UIntVariable>(name);
    public FloatVariable? GetFloat(string name) => GetVariable<FloatVariable>(name);
    public ArrayVariable? GetArray(string name) => GetVariable<ArrayVariable>(name);
    
    public bool TryGetVariable(string name, out PlcVariable? variable) => _variables.TryGetValue(name, out variable);
    
    public IReadOnlyDictionary<string, PlcVariable> GetAllVariables() => _variables;
    
    // === Subscriptions ===
    
    public IDisposable Subscribe<T>(string name, Action<T> handler)
    {
        var variable = GetVariable<PlcVariable>(name);
        if (variable == null)
            throw new ArgumentException($"Variable {name} not found");
        
        var wrapper = new Action<object>(v => handler((T)v));
        _subscriptions.AddOrUpdate(name, [wrapper], (_, list) => { list.Add(wrapper); return list; });
        
        return new Subscription(() => 
        {
            if (_subscriptions.TryGetValue(name, out var list))
                list.Remove(wrapper);
        });
    }
    
    public async Task<WriteResult> WriteAsync(string name, object value, CancellationToken ct = default)
    {
        if (!_variables.TryGetValue(name, out var variable))
            return WriteResult.Failure($"Variable {name} not found");
        
        if (variable.AccessLevel == VariableAccessLevel.Read)
            return WriteResult.Failure($"Variable {name} is read-only");
        
        try
        {
            var rawValues = variable.ConvertToRaw(value);
            await WriteMultipleRegistersAsync(variable.Address, rawValues, ct);
            variable.UpdateFromRaw(rawValues);
            NotifySubscribers(name, variable.GetValue());
            return WriteResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write variable {Name}", name);
            return WriteResult.Failure(ex.Message);
        }
    }
    
    public async Task<WriteResult> WritePulseAsync(string name, bool value, int durationMs, CancellationToken ct = default)
    {
        var result = await WriteAsync(name, value, ct);
        if (!result.Success) return result;
        
        await Task.Delay(durationMs, ct);
        return await WriteAsync(name, !value, ct);
    }
    
    // === RegisterMap Access ===
    
    public RegisterMap? GetRegisterMap() => _registerMap;
    
    public ushort GetRegisterAddress(string name)
    {
        return _registerMap!.Registers.TryGetValue(name, out var info) ? info.Address : (ushort)0;
    }
    
    public VariableType GetVariableType(string name)
    {
        return _registerMap!.Registers.TryGetValue(name, out var info) ? info.Type : VariableType.Int;
    }
    
    public async ValueTask DisposeAsync()
    {
        StopPolling();
        await DisconnectAsync();
        _modbusClient.Dispose();
    }
    
    private sealed class Subscription : IDisposable
    {
        private readonly Action _unsubscribe;
        public Subscription(Action unsubscribe) => _unsubscribe = unsubscribe;
        public void Dispose() => _unsubscribe();
    }
}

public record WriteResult(bool Success, string? ErrorMessage)
{
    public static WriteResult Success() => new(true, null);
    public static WriteResult Failure(string error) => new(false, error);
}