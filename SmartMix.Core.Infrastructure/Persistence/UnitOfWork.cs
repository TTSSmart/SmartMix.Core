using Microsoft.Extensions.Logging;
using SmartMix.Core.Application.Abstractions;
using SmartMix.Core.Infrastructure.Persistence.Database;
using MySql.Data.MySqlClient;

namespace SmartMix.Core.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ISmartMixDatabase _database;
    private readonly ILogger<UnitOfWork> _logger;
    private MySqlTransaction? _transaction;
    private bool _disposed;
    
    public UnitOfWork(ISmartMixDatabase database, ILogger<UnitOfWork> logger)
    {
        _database = database;
        _logger = logger;
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // В Dapper изменения сохраняются сразу при ExecuteAsync
        // Этот метод нужен для паттерна Unit of Work
        return await Task.FromResult(0);
    }
    
    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
            throw new InvalidOperationException("Transaction already started");
        
        var conn = _database.GetConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync(ct);
        
        _transaction = await conn.BeginTransactionAsync(ct);
        _logger.LogDebug("Transaction started");
    }
    
    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("No active transaction");
        
        await _transaction.CommitAsync(ct);
        _transaction.Dispose();
        _transaction = null;
        _logger.LogDebug("Transaction committed");
    }
    
    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("No active transaction");
        
        await _transaction.RollbackAsync(ct);
        _transaction.Dispose();
        _transaction = null;
        _logger.LogDebug("Transaction rolled back");
    }
    
    public MySqlTransaction? CurrentTransaction => _transaction;
    
    public void Dispose()
    {
        if (_disposed) return;
        
        _transaction?.Dispose();
        _disposed = true;
    }
}