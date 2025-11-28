using Isotyp.Application.Common;
using Isotyp.Application.DTOs;

namespace Isotyp.Application.Interfaces;

/// <summary>
/// Service for managing data sources.
/// </summary>
public interface IDataSourceService
{
    Task<Result<DataSourceDto>> CreateAsync(CreateDataSourceDto dto, string userId, CancellationToken cancellationToken = default);
    Task<Result<DataSourceDto>> UpdateAsync(Guid id, UpdateDataSourceDto dto, string userId, CancellationToken cancellationToken = default);
    Task<Result<DataSourceDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<DataSourceDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<DataSourceDto>>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Result> ActivateAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    Task<Result> DeactivateAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, string userId, CancellationToken cancellationToken = default);
}
