using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using SmartMix.Core.Application.Abstractions;
using SmartMix.Core.Infrastructure.Configuration;
using System.Data;

namespace SmartMix.Core.Infrastructure.Persistence.Database;

public class SmartMixDatabase : ISmartMixDatabase
{
    private readonly IOptions<DatabaseOptions> _options;
    private readonly MySqlConnection _connection;
    private bool _disposed;
    
    public SmartMixDatabase(IOptions<DatabaseOptions> options)
    {
        _options = options;
        _connection = new MySqlConnection(_options.Value.ConnectionString);
    }
    
    public IDbConnection GetConnection()
    {
        if (_connection.State != ConnectionState.Open)
            _connection.Open();
        return _connection;
    }
    
    public async Task<IDbConnection> GetConnectionAsync(CancellationToken ct = default)
    {
        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync(ct);
        return _connection;
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _connection.Dispose();
        _disposed = true;
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        await _connection.DisposeAsync();
        _disposed = true;
    }
}

public interface ISmartMixDatabase
{
    IDbConnection GetConnection();
    Task<IDbConnection> GetConnectionAsync(CancellationToken ct = default);
}