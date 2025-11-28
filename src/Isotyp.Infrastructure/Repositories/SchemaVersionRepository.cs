using Microsoft.EntityFrameworkCore;
using Isotyp.Core.Entities;
using Isotyp.Core.Enums;
using Isotyp.Core.Interfaces;
using Isotyp.Infrastructure.Data;

namespace Isotyp.Infrastructure.Repositories;

public class SchemaVersionRepository : Repository<SchemaVersion>, ISchemaVersionRepository
{
    public SchemaVersionRepository(IsotypDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<SchemaVersion>> GetByDataSourceIdAsync(Guid dataSourceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.DataSourceId == dataSourceId)
            .OrderByDescending(s => s.MajorVersion)
            .ThenByDescending(s => s.MinorVersion)
            .ThenByDescending(s => s.PatchVersion)
            .ToListAsync(cancellationToken);
    }

    public async Task<SchemaVersion?> GetLatestAppliedVersionAsync(Guid dataSourceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.DataSourceId == dataSourceId && s.Status == ApprovalStatus.Applied)
            .OrderByDescending(s => s.AppliedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SchemaVersion?> GetLatestVersionAsync(Guid dataSourceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.DataSourceId == dataSourceId)
            .OrderByDescending(s => s.MajorVersion)
            .ThenByDescending(s => s.MinorVersion)
            .ThenByDescending(s => s.PatchVersion)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SchemaVersion>> GetByStatusAsync(ApprovalStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.Status == status)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<SchemaVersion?> GetByVersionNumberAsync(Guid dataSourceId, int major, int minor, int patch, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => 
                s.DataSourceId == dataSourceId && 
                s.MajorVersion == major && 
                s.MinorVersion == minor && 
                s.PatchVersion == patch, 
                cancellationToken);
    }
}
