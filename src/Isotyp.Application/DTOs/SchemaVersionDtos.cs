using Isotyp.Core.Enums;

namespace Isotyp.Application.DTOs;

/// <summary>
/// DTO for creating a new schema version.
/// </summary>
public record CreateSchemaVersionDto(
    Guid DataSourceId,
    string SchemaDefinition,
    string ChangeDescription,
    string OrmMappings,
    string MigrationScript,
    string RollbackScript,
    Guid? ParentVersionId);

/// <summary>
/// DTO for schema version response.
/// </summary>
public record SchemaVersionDto(
    Guid Id,
    Guid DataSourceId,
    string VersionString,
    int MajorVersion,
    int MinorVersion,
    int PatchVersion,
    string SchemaDefinition,
    string ChangeDescription,
    ApprovalStatus Status,
    SchemaLockType LockType,
    Guid? ParentVersionId,
    string OrmMappings,
    DateTime? AppliedAt,
    string? AppliedBy,
    DateTime CreatedAt,
    string CreatedBy,
    int Version);

/// <summary>
/// DTO for applying a schema lock.
/// </summary>
public record ApplySchemaLockDto(
    Guid SchemaVersionId,
    SchemaLockType LockType);

/// <summary>
/// DTO for schema rollback request.
/// </summary>
public record SchemaRollbackDto(
    Guid SchemaVersionId,
    string Reason);
