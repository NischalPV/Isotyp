using Isotyp.Core.Enums;

namespace Isotyp.Application.DTOs;

/// <summary>
/// DTO for creating a new data source.
/// </summary>
public record CreateDataSourceDto(
    string Name,
    string Description,
    DataSourceType Type,
    string ConnectionStringReference);

/// <summary>
/// DTO for updating a data source.
/// </summary>
public record UpdateDataSourceDto(
    string Name,
    string Description);

/// <summary>
/// DTO for data source response (metadata only - no connection details).
/// </summary>
public record DataSourceDto(
    Guid Id,
    string Name,
    string Description,
    DataSourceType Type,
    bool IsActive,
    DateTime? LastConnectedAt,
    DateTime? LastValidatedAt,
    DateTime CreatedAt,
    string CreatedBy,
    int Version);
