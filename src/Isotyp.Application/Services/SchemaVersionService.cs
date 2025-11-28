using Isotyp.Application.Common;
using Isotyp.Application.DTOs;
using Isotyp.Application.Interfaces;
using Isotyp.Core.Entities;
using Isotyp.Core.Enums;
using Isotyp.Core.Exceptions;
using Isotyp.Core.Interfaces;

namespace Isotyp.Application.Services;

/// <summary>
/// Service implementation for managing schema versions with proper locking, versioning, and approval.
/// </summary>
public class SchemaVersionService : ISchemaVersionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public SchemaVersionService(IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<Result<SchemaVersionDto>> CreateAsync(CreateSchemaVersionDto dto, string userId, CancellationToken cancellationToken = default)
    {
        // Check data source exists
        var dataSource = await _unitOfWork.DataSources.GetByIdAsync(dto.DataSourceId, cancellationToken);
        if (dataSource == null)
            return Result<SchemaVersionDto>.Failure("Data source not found.");

        // Check parent version lock status if exists
        if (dto.ParentVersionId.HasValue)
        {
            var parentVersion = await _unitOfWork.SchemaVersions.GetByIdAsync(dto.ParentVersionId.Value, cancellationToken);
            if (parentVersion == null)
                return Result<SchemaVersionDto>.Failure("Parent schema version not found.");
            
            if (parentVersion.IsFullyLocked())
                throw new SchemaLockedException(parentVersion.Id, "Cannot create new version from a fully locked schema.");
        }

        // Calculate next version number
        var latestVersion = await _unitOfWork.SchemaVersions.GetLatestVersionAsync(dto.DataSourceId, cancellationToken);
        int major = 1, minor = 0, patch = 0;
        
        if (latestVersion != null)
        {
            major = latestVersion.MajorVersion;
            minor = latestVersion.MinorVersion;
            patch = latestVersion.PatchVersion + 1;
        }

        var schemaVersion = SchemaVersion.Create(
            dto.DataSourceId,
            major,
            minor,
            patch,
            dto.SchemaDefinition,
            dto.ChangeDescription,
            dto.OrmMappings,
            dto.MigrationScript,
            dto.RollbackScript,
            dto.ParentVersionId,
            userId);

        await _unitOfWork.SchemaVersions.AddAsync(schemaVersion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.Create,
            nameof(SchemaVersion),
            schemaVersion.Id,
            userId,
            userId,
            null,
            System.Text.Json.JsonSerializer.Serialize(MapToDto(schemaVersion)),
            null,
            null,
            null,
            cancellationToken);

        return Result<SchemaVersionDto>.Success(MapToDto(schemaVersion));
    }

    public async Task<Result<SchemaVersionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var schemaVersion = await _unitOfWork.SchemaVersions.GetByIdAsync(id, cancellationToken);
        if (schemaVersion == null)
            return Result<SchemaVersionDto>.Failure("Schema version not found.");

        return Result<SchemaVersionDto>.Success(MapToDto(schemaVersion));
    }

    public async Task<Result<IReadOnlyList<SchemaVersionDto>>> GetByDataSourceIdAsync(Guid dataSourceId, CancellationToken cancellationToken = default)
    {
        var versions = await _unitOfWork.SchemaVersions.GetByDataSourceIdAsync(dataSourceId, cancellationToken);
        return Result<IReadOnlyList<SchemaVersionDto>>.Success(versions.Select(MapToDto).ToList());
    }

    public async Task<Result<SchemaVersionDto?>> GetLatestAppliedAsync(Guid dataSourceId, CancellationToken cancellationToken = default)
    {
        var version = await _unitOfWork.SchemaVersions.GetLatestAppliedVersionAsync(dataSourceId, cancellationToken);
        return Result<SchemaVersionDto?>.Success(version != null ? MapToDto(version) : null);
    }

    public async Task<Result> SubmitForApprovalAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var schemaVersion = await _unitOfWork.SchemaVersions.GetByIdAsync(id, cancellationToken);
        if (schemaVersion == null)
            return Result.Failure("Schema version not found.");

        var stateBefore = System.Text.Json.JsonSerializer.Serialize(MapToDto(schemaVersion));

        schemaVersion.Submit(userId);
        await _unitOfWork.SchemaVersions.UpdateAsync(schemaVersion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.SchemaChangeProposed,
            nameof(SchemaVersion),
            schemaVersion.Id,
            userId,
            userId,
            stateBefore,
            System.Text.Json.JsonSerializer.Serialize(MapToDto(schemaVersion)),
            "Submitted for approval",
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ApproveAsync(Guid id, ApprovalLayer layer, string approverUserId, CancellationToken cancellationToken = default)
    {
        var schemaVersion = await _unitOfWork.SchemaVersions.GetByIdAsync(id, cancellationToken);
        if (schemaVersion == null)
            return Result.Failure("Schema version not found.");

        var stateBefore = System.Text.Json.JsonSerializer.Serialize(MapToDto(schemaVersion));

        switch (layer)
        {
            case ApprovalLayer.Technical:
                schemaVersion.ApproveTechnical(approverUserId);
                break;
            case ApprovalLayer.Business:
                schemaVersion.ApproveBusiness(approverUserId);
                break;
            case ApprovalLayer.DataGovernance:
                schemaVersion.ApproveGovernance(approverUserId);
                break;
            default:
                return Result.Failure($"Unknown approval layer: {layer}");
        }

        await _unitOfWork.SchemaVersions.UpdateAsync(schemaVersion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.SchemaChangeApproved,
            nameof(SchemaVersion),
            schemaVersion.Id,
            approverUserId,
            approverUserId,
            stateBefore,
            System.Text.Json.JsonSerializer.Serialize(MapToDto(schemaVersion)),
            $"Approved at layer: {layer}",
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RejectAsync(Guid id, string reason, string userId, CancellationToken cancellationToken = default)
    {
        var schemaVersion = await _unitOfWork.SchemaVersions.GetByIdAsync(id, cancellationToken);
        if (schemaVersion == null)
            return Result.Failure("Schema version not found.");

        var stateBefore = System.Text.Json.JsonSerializer.Serialize(MapToDto(schemaVersion));

        schemaVersion.Reject(userId);
        await _unitOfWork.SchemaVersions.UpdateAsync(schemaVersion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.SchemaChangeRejected,
            nameof(SchemaVersion),
            schemaVersion.Id,
            userId,
            userId,
            stateBefore,
            System.Text.Json.JsonSerializer.Serialize(MapToDto(schemaVersion)),
            reason,
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ApplyAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var schemaVersion = await _unitOfWork.SchemaVersions.GetByIdAsync(id, cancellationToken);
        if (schemaVersion == null)
            return Result.Failure("Schema version not found.");

        if (schemaVersion.Status != ApprovalStatus.FullyApproved)
            return Result.Failure("Can only apply fully approved schema versions.");

        var stateBefore = System.Text.Json.JsonSerializer.Serialize(MapToDto(schemaVersion));

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Mark as applied - in real implementation, this would trigger actual DB migration
            schemaVersion.MarkAsApplied(userId, DateTime.UtcNow);
            await _unitOfWork.SchemaVersions.UpdateAsync(schemaVersion, cancellationToken);
            
            // Update data source last validated time
            var dataSource = await _unitOfWork.DataSources.GetByIdAsync(schemaVersion.DataSourceId, cancellationToken);
            if (dataSource != null)
            {
                dataSource.RecordValidation(DateTime.UtcNow);
                await _unitOfWork.DataSources.UpdateAsync(dataSource, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.SchemaChangeApplied,
                nameof(SchemaVersion),
                schemaVersion.Id,
                userId,
                userId,
                stateBefore,
                System.Text.Json.JsonSerializer.Serialize(MapToDto(schemaVersion)),
                "Schema version applied successfully",
                null,
                null,
                cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            
            await _auditService.LogAsync(
                AuditAction.SchemaChangeApplied,
                nameof(SchemaVersion),
                schemaVersion.Id,
                userId,
                userId,
                stateBefore,
                null,
                $"Failed to apply schema version: {ex.Message}",
                null,
                null,
                cancellationToken);

            throw new RollbackException(id, $"Failed to apply schema version: {ex.Message}", ex);
        }
    }

    public async Task<Result> RollbackAsync(Guid id, string reason, string userId, CancellationToken cancellationToken = default)
    {
        var schemaVersion = await _unitOfWork.SchemaVersions.GetByIdAsync(id, cancellationToken);
        if (schemaVersion == null)
            return Result.Failure("Schema version not found.");

        if (schemaVersion.Status != ApprovalStatus.Applied)
            return Result.Failure("Can only rollback applied schema versions.");

        var stateBefore = System.Text.Json.JsonSerializer.Serialize(MapToDto(schemaVersion));
        var correlationId = Guid.NewGuid();

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Mark as rolled back
            schemaVersion.MarkAsRolledBack(userId, DateTime.UtcNow);
            await _unitOfWork.SchemaVersions.UpdateAsync(schemaVersion, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.SchemaChangeRolledBack,
                nameof(SchemaVersion),
                schemaVersion.Id,
                userId,
                userId,
                stateBefore,
                System.Text.Json.JsonSerializer.Serialize(MapToDto(schemaVersion)),
                $"Rolled back. Reason: {reason}",
                null,
                correlationId,
                cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw new RollbackException(id, $"Failed to rollback schema version: {ex.Message}", ex);
        }
    }

    public async Task<Result> ApplyLockAsync(Guid id, SchemaLockType lockType, string userId, CancellationToken cancellationToken = default)
    {
        var schemaVersion = await _unitOfWork.SchemaVersions.GetByIdAsync(id, cancellationToken);
        if (schemaVersion == null)
            return Result.Failure("Schema version not found.");

        var stateBefore = System.Text.Json.JsonSerializer.Serialize(MapToDto(schemaVersion));

        schemaVersion.ApplyLock(lockType, userId);
        await _unitOfWork.SchemaVersions.UpdateAsync(schemaVersion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.SchemaLockApplied,
            nameof(SchemaVersion),
            schemaVersion.Id,
            userId,
            userId,
            stateBefore,
            System.Text.Json.JsonSerializer.Serialize(MapToDto(schemaVersion)),
            $"Lock type: {lockType}",
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RemoveLockAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var schemaVersion = await _unitOfWork.SchemaVersions.GetByIdAsync(id, cancellationToken);
        if (schemaVersion == null)
            return Result.Failure("Schema version not found.");

        var stateBefore = System.Text.Json.JsonSerializer.Serialize(MapToDto(schemaVersion));

        schemaVersion.RemoveLock(userId);
        await _unitOfWork.SchemaVersions.UpdateAsync(schemaVersion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.SchemaLockRemoved,
            nameof(SchemaVersion),
            schemaVersion.Id,
            userId,
            userId,
            stateBefore,
            System.Text.Json.JsonSerializer.Serialize(MapToDto(schemaVersion)),
            "Lock removed",
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    private static SchemaVersionDto MapToDto(SchemaVersion entity)
    {
        return new SchemaVersionDto(
            entity.Id,
            entity.DataSourceId,
            entity.GetVersionString(),
            entity.MajorVersion,
            entity.MinorVersion,
            entity.PatchVersion,
            entity.SchemaDefinition,
            entity.ChangeDescription,
            entity.Status,
            entity.LockType,
            entity.ParentVersionId,
            entity.OrmMappings,
            entity.AppliedAt,
            entity.AppliedBy,
            entity.CreatedAt,
            entity.CreatedBy,
            entity.Version);
    }
}
