using Isotyp.Application.Common;
using Isotyp.Application.DTOs;
using Isotyp.Core.Enums;

namespace Isotyp.Application.Interfaces;

/// <summary>
/// Service for managing schema versions with proper locking and versioning.
/// </summary>
public interface ISchemaVersionService
{
    Task<Result<SchemaVersionDto>> CreateAsync(CreateSchemaVersionDto dto, string userId, CancellationToken cancellationToken = default);
    Task<Result<SchemaVersionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<SchemaVersionDto>>> GetByDataSourceIdAsync(Guid dataSourceId, CancellationToken cancellationToken = default);
    Task<Result<SchemaVersionDto?>> GetLatestAppliedAsync(Guid dataSourceId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Submit a schema version for approval workflow.
    /// </summary>
    Task<Result> SubmitForApprovalAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Process approval at a specific layer.
    /// </summary>
    Task<Result> ApproveAsync(Guid id, ApprovalLayer layer, string approverUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reject a schema version.
    /// </summary>
    Task<Result> RejectAsync(Guid id, string reason, string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Apply a fully approved schema version. Updates both DB and ORM.
    /// </summary>
    Task<Result> ApplyAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Rollback an applied schema version. Must be fully auditable.
    /// </summary>
    Task<Result> RollbackAsync(Guid id, string reason, string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Apply a lock to a schema version. Respects existing locks.
    /// </summary>
    Task<Result> ApplyLockAsync(Guid id, SchemaLockType lockType, string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove a lock from a schema version. Requires proper authorization.
    /// </summary>
    Task<Result> RemoveLockAsync(Guid id, string userId, CancellationToken cancellationToken = default);
}
