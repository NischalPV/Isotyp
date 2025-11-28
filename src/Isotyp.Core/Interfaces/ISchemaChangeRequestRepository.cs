using Isotyp.Core.Entities;
using Isotyp.Core.Enums;

namespace Isotyp.Core.Interfaces;

/// <summary>
/// Repository for schema change requests.
/// </summary>
public interface ISchemaChangeRequestRepository : IRepository<SchemaChangeRequest>
{
    Task<IReadOnlyList<SchemaChangeRequest>> GetBySchemaVersionIdAsync(Guid schemaVersionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SchemaChangeRequest>> GetByStatusAsync(ApprovalStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SchemaChangeRequest>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SchemaChangeRequest>> GetAiSuggestedAsync(CancellationToken cancellationToken = default);
}
