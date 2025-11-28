using Microsoft.EntityFrameworkCore.Storage;
using Isotyp.Core.Interfaces;
using Isotyp.Infrastructure.Data;
using Isotyp.Infrastructure.Repositories;

namespace Isotyp.Infrastructure;

/// <summary>
/// Unit of work implementation for coordinating transactions.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly IsotypDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    private IDataSourceRepository? _dataSourceRepository;
    private ISchemaVersionRepository? _schemaVersionRepository;
    private ISchemaChangeRequestRepository? _schemaChangeRequestRepository;
    private IAiSuggestionRepository? _aiSuggestionRepository;
    private IAuditLogRepository? _auditLogRepository;
    private IAgentRepository? _agentRepository;
    private IDataValidationRepository? _dataValidationRepository;

    public UnitOfWork(IsotypDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IDataSourceRepository DataSources => 
        _dataSourceRepository ??= new DataSourceRepository(_context);

    public ISchemaVersionRepository SchemaVersions => 
        _schemaVersionRepository ??= new SchemaVersionRepository(_context);

    public ISchemaChangeRequestRepository SchemaChangeRequests => 
        _schemaChangeRequestRepository ??= new SchemaChangeRequestRepository(_context);

    public IAiSuggestionRepository AiSuggestions => 
        _aiSuggestionRepository ??= new AiSuggestionRepository(_context);

    public IAuditLogRepository AuditLogs => 
        _auditLogRepository ??= new AuditLogRepository(_context);

    public IAgentRepository Agents => 
        _agentRepository ??= new AgentRepository(_context);

    public IDataValidationRepository DataValidations => 
        _dataValidationRepository ??= new DataValidationRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction in progress.");

        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction in progress.");

        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
        _disposed = true;
    }
}
