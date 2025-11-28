using Microsoft.EntityFrameworkCore;
using Isotyp.Core.Entities;
using Isotyp.Core.Enums;
using Isotyp.Core.Interfaces;
using Isotyp.Infrastructure.Data;

namespace Isotyp.Infrastructure.Repositories;

public class DataSourceRepository : Repository<DataSource>, IDataSourceRepository
{
    public DataSourceRepository(IsotypDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<DataSource>> GetActiveDataSourcesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(d => d.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DataSource>> GetByTypeAsync(DataSourceType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(d => d.Type == type).ToListAsync(cancellationToken);
    }

    public async Task<DataSource?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(d => d.Name == name, cancellationToken);
    }
}
