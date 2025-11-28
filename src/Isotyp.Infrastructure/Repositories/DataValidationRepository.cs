using Microsoft.EntityFrameworkCore;
using Isotyp.Core.Entities;
using Isotyp.Core.Enums;
using Isotyp.Core.Interfaces;
using Isotyp.Infrastructure.Data;

namespace Isotyp.Infrastructure.Repositories;

public class DataValidationRepository : Repository<DataValidation>, IDataValidationRepository
{
    public DataValidationRepository(IsotypDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<DataValidation>> GetByDataSourceIdAsync(Guid dataSourceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(v => v.DataSourceId == dataSourceId)
            .OrderByDescending(v => v.ExecutedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DataValidation>> GetBySchemaVersionIdAsync(Guid schemaVersionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(v => v.SchemaVersionId == schemaVersionId)
            .OrderByDescending(v => v.ExecutedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<DataValidation?> GetLatestByDataSourceIdAsync(Guid dataSourceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(v => v.DataSourceId == dataSourceId)
            .OrderByDescending(v => v.ExecutedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DataValidation>> GetByStatusAsync(ValidationStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(v => v.Status == status)
            .OrderByDescending(v => v.ExecutedAt)
            .ToListAsync(cancellationToken);
    }
}
