using Isotyp.Core.Entities;
using Isotyp.Core.Enums;

namespace Isotyp.Core.Interfaces;

/// <summary>
/// Repository for data validations.
/// </summary>
public interface IDataValidationRepository : IRepository<DataValidation>
{
    Task<IReadOnlyList<DataValidation>> GetByDataSourceIdAsync(Guid dataSourceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DataValidation>> GetBySchemaVersionIdAsync(Guid schemaVersionId, CancellationToken cancellationToken = default);
    Task<DataValidation?> GetLatestByDataSourceIdAsync(Guid dataSourceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DataValidation>> GetByStatusAsync(ValidationStatus status, CancellationToken cancellationToken = default);
}
