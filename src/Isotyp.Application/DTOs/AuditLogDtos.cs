using Isotyp.Core.Enums;

namespace Isotyp.Application.DTOs;

/// <summary>
/// DTO for audit log response.
/// </summary>
public record AuditLogDto(
    Guid Id,
    AuditAction Action,
    string EntityType,
    Guid EntityId,
    string UserId,
    string UserName,
    string? StateBefore,
    string? StateAfter,
    string? AdditionalData,
    string? ClientIdentifier,
    Guid? CorrelationId,
    DateTime CreatedAt);

/// <summary>
/// DTO for querying audit logs.
/// </summary>
public record AuditLogQueryDto(
    string? EntityType = null,
    Guid? EntityId = null,
    string? UserId = null,
    AuditAction? Action = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    Guid? CorrelationId = null,
    int PageNumber = 1,
    int PageSize = 50);
