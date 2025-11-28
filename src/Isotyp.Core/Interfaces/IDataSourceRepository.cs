using Isotyp.Core.Entities;
using Isotyp.Core.Enums;

namespace Isotyp.Core.Interfaces;

/// <summary>
/// Repository for data sources.
/// </summary>
public interface IDataSourceRepository : IRepository<DataSource>
{
    Task<IReadOnlyList<DataSource>> GetActiveDataSourcesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DataSource>> GetByTypeAsync(DataSourceType type, CancellationToken cancellationToken = default);
    Task<DataSource?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
