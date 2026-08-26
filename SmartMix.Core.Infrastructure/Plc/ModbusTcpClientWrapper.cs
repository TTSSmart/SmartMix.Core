using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartMix.Core.Application.Abstractions;
using SmartMix.Core.Infrastructure.Configuration;

namespace SmartMix.Core.Infrastructure.Plc;

/// <summary>
/// Обертка над ModbusTcpClient для соответствия IPlcClient
/// </summary>
public class ModbusTcpClientWrapper : IPlcClient, IDisposable
{
    private readonly ModbusTcpClient _client;
    private readonly IOptions<PlcOptions> _options;
    private readonly ILogger<ModbusTcpClientWrapper> _logger;
    private bool _disposed;
    
    public event Action<bool>? ConnectionChanged;
    public event Action<ushort[], ushort>? RegistersUpdated;
    
    public bool IsConnected => _client.Connected;
    
    public ModbusTcpClientWrapper(
        IOptions<PlcOptions> options,
        ILogger<ModbusTcpClientWrapper> logger)
    {
        _options = options;
        _logger = logger;
        _client = new ModbusTcpClient(options.Value.Host, options.Value.Port);
        
        _client.OnException += (s, e) => _logger.LogError(e.Exception, "Modbus exception");
        _client.OnResponse += (s, e) => { /* Можно логировать ответы */ };
    }
    
    public async Task ConnectAsync(CancellationToken ct = default)
    {
        var connected = await Task.Run(() => _client.TryConnect(), ct);
        
        if (!connected)
            throw new InvalidOperationException($"Failed to connect to PLC {_options.Value.Host}:{_options.Value.Port}");
        
        _logger.LogInformation("Connected to PLC {Host}:{Port}", _options.Value.Host, _options.Value.Port);
        ConnectionChanged?.Invoke(true);
    }
    
    public async Task DisconnectAsync()
    {
        await Task.Run(() => _client.Disconnect());
        _logger.LogInformation("Disconnected from PLC");
        ConnectionChanged?.Invoke(false);
    }
    
    public async Task<ushort[]> ReadHoldingRegistersAsync(ushort startAddress, ushort count, CancellationToken ct = default)
    {
        if (!_client.Connected)
            throw new InvalidOperationException("Not connected to PLC");
        
        return await Task.Run(() =>
        {
            byte[] response = Array.Empty<byte>();
            _client.ReadHoldingRegister(0, _options.Value.UnitId, startAddress, count, ref response);
            
            if (response.Length < 2)
                throw new InvalidOperationException("Invalid response length");
            
            // Парсинг ответа Modbus: байты 9+ содержат данные
            var dataLength = response[8];
            var result = new ushort[count];
            
            for (int i = 0; i < count && (9 + i * 2) < response.Length; i++)
            {
                result[i] = (ushort)(response[9 + i * 2] << 8 | response[9 + i * 2 + 1]);
            }
            
            return result;
        }, ct);
    }
    
    public async Task WriteSingleRegisterAsync(ushort address, ushort value, CancellationToken ct = default)
    {
        if (!_client.Connected)
            throw new InvalidOperationException("Not connected to PLC");
        
        await Task.Run(() =>
        {
            byte[] valueBytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(valueBytes);
            
            byte[] result = Array.Empty<byte>();
            _client.WriteSingleRegister(0, _options.Value.UnitId, address, valueBytes, ref result);
        }, ct);
    }
    
    public async Task WriteMultipleRegistersAsync(ushort startAddress, ushort[] values, CancellationToken ct = default)
    {
        if (!_client.Connected)
            throw new InvalidOperationException("Not connected to PLC");
        
        await Task.Run(() =>
        {
            var bytes = new byte[values.Length * 2];
            for (int i = 0; i < values.Length; i++)
            {
                var valBytes = BitConverter.GetBytes(values[i]);
                if (BitConverter.IsLittleEndian) Array.Reverse(valBytes);
                Array.Copy(valBytes, 0, bytes, i * 2, 2);
            }
            
            byte[] result = Array.Empty<byte>();
            _client.WriteMultipleRegister(0, _options.Value.UnitId, startAddress, bytes, ref result);
        }, ct);
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        
        _client.Disconnect();
        _client.Dispose();
        _disposed = true;
    }
}