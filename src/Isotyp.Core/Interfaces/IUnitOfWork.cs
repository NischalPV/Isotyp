namespace Isotyp.Core.Interfaces;

/// <summary>
/// Unit of work pattern for coordinating transactions across repositories.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IDataSourceRepository DataSources { get; }
    ISchemaVersionRepository SchemaVersions { get; }
    ISchemaChangeRequestRepository SchemaChangeRequests { get; }
    IAiSuggestionRepository AiSuggestions { get; }
    IAuditLogRepository AuditLogs { get; }
    IAgentRepository Agents { get; }
    IDataValidationRepository DataValidations { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
