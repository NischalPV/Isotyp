using Isotyp.Application.Common;
using Isotyp.Application.DTOs;
using Isotyp.Core.Enums;

namespace Isotyp.Application.Interfaces;

/// <summary>
/// Service for querying audit logs. Audit logs are immutable and fully traceable.
/// </summary>
public interface IAuditService
{
    Task<Result<IReadOnlyList<AuditLogDto>>> QueryAsync(AuditLogQueryDto query, CancellationToken cancellationToken = default);
    Task<Result<AuditLogDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AuditLogDto>>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AuditLogDto>>> GetByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Internal method to log an audit entry. Should be called by other services.
    /// </summary>
    Task LogAsync(
        AuditAction action,
        string entityType,
        Guid entityId,
        string userId,
        string userName,
        string? stateBefore,
        string? stateAfter,
        string? additionalData,
        string? clientIdentifier,
        Guid? correlationId,
        CancellationToken cancellationToken = default);
}
