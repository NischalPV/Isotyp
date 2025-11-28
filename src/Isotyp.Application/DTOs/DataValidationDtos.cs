using Isotyp.Core.Enums;

namespace Isotyp.Application.DTOs;

/// <summary>
/// DTO for creating a data validation.
/// </summary>
public record CreateDataValidationDto(
    Guid DataSourceId,
    Guid SchemaVersionId,
    Guid? AgentId);

/// <summary>
/// DTO for completing a data validation.
/// </summary>
public record CompleteDataValidationDto(
    Guid ValidationId,
    long TotalRecords,
    long PassedRecords,
    long FailedRecords,
    long WarningRecords,
    string Errors,
    string Warnings,
    string Summary,
    long DurationMs);

/// <summary>
/// DTO for data validation response.
/// </summary>
public record DataValidationDto(
    Guid Id,
    Guid DataSourceId,
    Guid SchemaVersionId,
    Guid? AgentId,
    ValidationStatus Status,
    DateTime ExecutedAt,
    long DurationMs,
    long TotalRecords,
    long PassedRecords,
    long FailedRecords,
    long WarningRecords,
    string Errors,
    string Warnings,
    string Summary,
    double PassRate,
    DateTime CreatedAt);
