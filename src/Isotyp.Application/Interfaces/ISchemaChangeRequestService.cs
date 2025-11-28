using Isotyp.Application.Common;
using Isotyp.Application.DTOs;

namespace Isotyp.Application.Interfaces;

/// <summary>
/// Service for managing schema change requests with multi-layer approval.
/// </summary>
public interface ISchemaChangeRequestService
{
    Task<Result<SchemaChangeRequestDto>> CreateAsync(CreateSchemaChangeRequestDto dto, string userId, CancellationToken cancellationToken = default);
    Task<Result<SchemaChangeRequestDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<SchemaChangeRequestDto>>> GetBySchemaVersionIdAsync(Guid schemaVersionId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<SchemaChangeRequestDto>>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Submit a change request with justification.
    /// </summary>
    Task<Result> SubmitAsync(SubmitChangeRequestDto dto, string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Process approval at a specific layer. All layers required for full approval.
    /// </summary>
    Task<Result> ApproveAsync(ApproveChangeRequestDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Apply a fully approved change request. Creates new schema version.
    /// </summary>
    Task<Result<SchemaVersionDto>> ApplyAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reject a change request with reason.
    /// </summary>
    Task<Result> RejectAsync(Guid id, string reason, string userId, CancellationToken cancellationToken = default);
}
