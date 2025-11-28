using Isotyp.Application.Common;
using Isotyp.Application.DTOs;

namespace Isotyp.Application.Interfaces;

/// <summary>
/// Service for data validation operations.
/// </summary>
public interface IDataValidationService
{
    Task<Result<DataValidationDto>> CreateAsync(CreateDataValidationDto dto, string userId, CancellationToken cancellationToken = default);
    Task<Result<DataValidationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<DataValidationDto>>> GetByDataSourceIdAsync(Guid dataSourceId, CancellationToken cancellationToken = default);
    Task<Result<DataValidationDto?>> GetLatestByDataSourceIdAsync(Guid dataSourceId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Start validation execution.
    /// </summary>
    Task<Result> StartAsync(Guid validationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Complete validation with results.
    /// </summary>
    Task<Result> CompleteAsync(CompleteDataValidationDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Fail validation with error.
    /// </summary>
    Task<Result> FailAsync(Guid validationId, string errorMessage, long durationMs, CancellationToken cancellationToken = default);
}
