using Isotyp.Application.Common;
using Isotyp.Application.DTOs;
using Isotyp.Application.Interfaces;
using Isotyp.Core.Entities;
using Isotyp.Core.Enums;
using Isotyp.Core.Interfaces;

namespace Isotyp.Application.Services;

/// <summary>
/// Service for audit log queries. Audit logs are immutable and fully traceable.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<IReadOnlyList<AuditLogDto>>> QueryAsync(AuditLogQueryDto query, CancellationToken cancellationToken = default)
    {
        var logs = await _unitOfWork.AuditLogs.GetAllAsync(cancellationToken);
        
        // Apply filters
        var filtered = logs.AsEnumerable();
        
        if (!string.IsNullOrEmpty(query.EntityType))
            filtered = filtered.Where(l => l.EntityType == query.EntityType);
        
        if (query.EntityId.HasValue)
            filtered = filtered.Where(l => l.EntityId == query.EntityId.Value);
        
        if (!string.IsNullOrEmpty(query.UserId))
            filtered = filtered.Where(l => l.UserId == query.UserId);
        
        if (query.Action.HasValue)
            filtered = filtered.Where(l => l.Action == query.Action.Value);
        
        if (query.FromDate.HasValue)
            filtered = filtered.Where(l => l.CreatedAt >= query.FromDate.Value);
        
        if (query.ToDate.HasValue)
            filtered = filtered.Where(l => l.CreatedAt <= query.ToDate.Value);
        
        if (query.CorrelationId.HasValue)
            filtered = filtered.Where(l => l.CorrelationId == query.CorrelationId.Value);

        // Apply pagination
        var result = filtered
            .OrderByDescending(l => l.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<AuditLogDto>>.Success(result);
    }

    public async Task<Result<AuditLogDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var log = await _unitOfWork.AuditLogs.GetByIdAsync(id, cancellationToken);
        if (log == null)
            return Result<AuditLogDto>.Failure("Audit log not found.");

        return Result<AuditLogDto>.Success(MapToDto(log));
    }

    public async Task<Result<IReadOnlyList<AuditLogDto>>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        var logs = await _unitOfWork.AuditLogs.GetByEntityAsync(entityType, entityId, cancellationToken);
        return Result<IReadOnlyList<AuditLogDto>>.Success(logs.Select(MapToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<AuditLogDto>>> GetByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default)
    {
        var logs = await _unitOfWork.AuditLogs.GetByCorrelationIdAsync(correlationId, cancellationToken);
        return Result<IReadOnlyList<AuditLogDto>>.Success(logs.Select(MapToDto).ToList());
    }

    public async Task LogAsync(
        AuditAction action,
        string entityType,
        Guid entityId,
        string userId,
        string userName,
        string? stateBefore,
        string? stateAfter,
        string? additionalData,
        string? clientIdentifier,
        Guid? correlationId,
        CancellationToken cancellationToken = default)
    {
        var auditLog = AuditLog.Create(
            action,
            entityType,
            entityId,
            userId,
            userName,
            stateBefore,
            stateAfter,
            additionalData,
            clientIdentifier,
            correlationId);

        await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static AuditLogDto MapToDto(AuditLog entity)
    {
        return new AuditLogDto(
            entity.Id,
            entity.Action,
            entity.EntityType,
            entity.EntityId,
            entity.UserId,
            entity.UserName,
            entity.StateBefore,
            entity.StateAfter,
            entity.AdditionalData,
            entity.ClientIdentifier,
            entity.CorrelationId,
            entity.CreatedAt);
    }
}
