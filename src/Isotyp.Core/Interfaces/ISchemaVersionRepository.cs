using Isotyp.Core.Entities;
using Isotyp.Core.Enums;

namespace Isotyp.Core.Interfaces;

/// <summary>
/// Repository for schema versions.
/// </summary>
public interface ISchemaVersionRepository : IRepository<SchemaVersion>
{
    Task<IReadOnlyList<SchemaVersion>> GetByDataSourceIdAsync(Guid dataSourceId, CancellationToken cancellationToken = default);
    Task<SchemaVersion?> GetLatestAppliedVersionAsync(Guid dataSourceId, CancellationToken cancellationToken = default);
    Task<SchemaVersion?> GetLatestVersionAsync(Guid dataSourceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SchemaVersion>> GetByStatusAsync(ApprovalStatus status, CancellationToken cancellationToken = default);
    Task<SchemaVersion?> GetByVersionNumberAsync(Guid dataSourceId, int major, int minor, int patch, CancellationToken cancellationToken = default);
}
