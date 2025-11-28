using Isotyp.Core.Entities;
using Isotyp.Core.Enums;

namespace Isotyp.Core.Interfaces;

/// <summary>
/// Repository for audit logs. Audit logs are immutable - no update or delete.
/// </summary>
public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByActionAsync(AuditAction action, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
