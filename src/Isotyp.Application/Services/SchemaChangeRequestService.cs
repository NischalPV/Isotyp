using Isotyp.Application.Common;
using Isotyp.Application.DTOs;
using Isotyp.Application.Interfaces;
using Isotyp.Core.Entities;
using Isotyp.Core.Enums;
using Isotyp.Core.Interfaces;

namespace Isotyp.Application.Services;

/// <summary>
/// Service for managing schema change requests with multi-layer approval.
/// All changes require explicit human approval - never auto-applied.
/// </summary>
public class SchemaChangeRequestService : ISchemaChangeRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ISchemaVersionService _schemaVersionService;

    public SchemaChangeRequestService(
        IUnitOfWork unitOfWork, 
        IAuditService auditService,
        ISchemaVersionService schemaVersionService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _schemaVersionService = schemaVersionService ?? throw new ArgumentNullException(nameof(schemaVersionService));
    }

    public async Task<Result<SchemaChangeRequestDto>> CreateAsync(CreateSchemaChangeRequestDto dto, string userId, CancellationToken cancellationToken = default)
    {
        // Verify schema version exists and can be modified
        var schemaVersion = await _unitOfWork.SchemaVersions.GetByIdAsync(dto.SchemaVersionId, cancellationToken);
        if (schemaVersion == null)
            return Result<SchemaChangeRequestDto>.Failure("Schema version not found.");

        // Check lock status
        if (schemaVersion.IsFullyLocked())
            return Result<SchemaChangeRequestDto>.Failure("Schema version is locked and cannot be modified.");

        // Check if additive-only lock applies to destructive changes
        if (schemaVersion.AllowsOnlyAdditive())
        {
            var isDestructive = dto.ChangeType == ChangeType.RemoveTable ||
                               dto.ChangeType == ChangeType.RemoveColumn ||
                               dto.ChangeType == ChangeType.ModifyColumn;
            if (isDestructive)
                return Result<SchemaChangeRequestDto>.Failure("Schema version only allows additive changes. Destructive changes are not permitted.");
        }

        var changeRequest = SchemaChangeRequest.Create(
            dto.SchemaVersionId,
            dto.ChangeType,
            dto.ChangeDetails,
            dto.Description,
            string.Empty, // Justification added on submit
            dto.ImpactAnalysis,
            dto.IsAiSuggested,
            dto.AiConfidenceScore,
            userId);

        await _unitOfWork.SchemaChangeRequests.AddAsync(changeRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.SchemaChangeProposed,
            nameof(SchemaChangeRequest),
            changeRequest.Id,
            userId,
            userId,
            null,
            System.Text.Json.JsonSerializer.Serialize(MapToDto(changeRequest)),
            dto.IsAiSuggested ? "AI-suggested change" : "Manual change request",
            null,
            null,
            cancellationToken);

        return Result<SchemaChangeRequestDto>.Success(MapToDto(changeRequest));
    }

    public async Task<Result<SchemaChangeRequestDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var changeRequest = await _unitOfWork.SchemaChangeRequests.GetByIdAsync(id, cancellationToken);
        if (changeRequest == null)
            return Result<SchemaChangeRequestDto>.Failure("Change request not found.");

        return Result<SchemaChangeRequestDto>.Success(MapToDto(changeRequest));
    }

    public async Task<Result<IReadOnlyList<SchemaChangeRequestDto>>> GetBySchemaVersionIdAsync(Guid schemaVersionId, CancellationToken cancellationToken = default)
    {
        var requests = await _unitOfWork.SchemaChangeRequests.GetBySchemaVersionIdAsync(schemaVersionId, cancellationToken);
        return Result<IReadOnlyList<SchemaChangeRequestDto>>.Success(requests.Select(MapToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<SchemaChangeRequestDto>>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default)
    {
        var requests = await _unitOfWork.SchemaChangeRequests.GetPendingApprovalsAsync(cancellationToken);
        return Result<IReadOnlyList<SchemaChangeRequestDto>>.Success(requests.Select(MapToDto).ToList());
    }

    public async Task<Result> SubmitAsync(SubmitChangeRequestDto dto, string userId, CancellationToken cancellationToken = default)
    {
        var changeRequest = await _unitOfWork.SchemaChangeRequests.GetByIdAsync(dto.ChangeRequestId, cancellationToken);
        if (changeRequest == null)
            return Result.Failure("Change request not found.");

        var stateBefore = System.Text.Json.JsonSerializer.Serialize(MapToDto(changeRequest));

        changeRequest.Submit(dto.Justification, userId);
        await _unitOfWork.SchemaChangeRequests.UpdateAsync(changeRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.SchemaChangeProposed,
            nameof(SchemaChangeRequest),
            changeRequest.Id,
            userId,
            userId,
            stateBefore,
            System.Text.Json.JsonSerializer.Serialize(MapToDto(changeRequest)),
            "Submitted for approval",
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ApproveAsync(ApproveChangeRequestDto dto, CancellationToken cancellationToken = default)
    {
        var changeRequest = await _unitOfWork.SchemaChangeRequests.GetByIdAsync(dto.ChangeRequestId, cancellationToken);
        if (changeRequest == null)
            return Result.Failure("Change request not found.");

        if (!changeRequest.IsReadyForApproval() && changeRequest.Status != ApprovalStatus.TechnicalApproved 
            && changeRequest.Status != ApprovalStatus.BusinessApproved)
            return Result.Failure("Change request is not ready for approval.");

        var stateBefore = System.Text.Json.JsonSerializer.Serialize(MapToDto(changeRequest));

        var approval = SchemaChangeApproval.Create(
            dto.ChangeRequestId,
            dto.Layer,
            dto.IsApproved,
            dto.ApproverUserId,
            dto.ApproverName,
            dto.Comments,
            dto.ApproverUserId);

        changeRequest.AddApproval(approval);
        await _unitOfWork.SchemaChangeRequests.UpdateAsync(changeRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            dto.IsApproved ? AuditAction.SchemaChangeApproved : AuditAction.SchemaChangeRejected,
            nameof(SchemaChangeRequest),
            changeRequest.Id,
            dto.ApproverUserId,
            dto.ApproverName,
            stateBefore,
            System.Text.Json.JsonSerializer.Serialize(MapToDto(changeRequest)),
            $"Layer: {dto.Layer}, Approved: {dto.IsApproved}, Comments: {dto.Comments}",
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result<SchemaVersionDto>> ApplyAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        var changeRequest = await _unitOfWork.SchemaChangeRequests.GetByIdAsync(id, cancellationToken);
        if (changeRequest == null)
            return Result<SchemaVersionDto>.Failure("Change request not found.");

        if (changeRequest.Status != ApprovalStatus.FullyApproved)
            return Result<SchemaVersionDto>.Failure("Can only apply fully approved change requests.");

        // Get the current schema version
        var currentVersion = await _unitOfWork.SchemaVersions.GetByIdAsync(changeRequest.SchemaVersionId, cancellationToken);
        if (currentVersion == null)
            return Result<SchemaVersionDto>.Failure("Associated schema version not found.");

        var correlationId = Guid.NewGuid();

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Create new schema version with the changes applied
            // In a real implementation, this would merge the change details into the schema
            var newVersionResult = await _schemaVersionService.CreateAsync(
                new CreateSchemaVersionDto(
                    currentVersion.DataSourceId,
                    currentVersion.SchemaDefinition, // Would be modified based on changeRequest.ChangeDetails
                    $"Applied change: {changeRequest.Description}",
                    currentVersion.OrmMappings, // Would be updated based on changes
                    currentVersion.MigrationScript, // Would include new migration
                    currentVersion.RollbackScript, // Would include rollback for new changes
                    currentVersion.Id),
                userId,
                cancellationToken);

            if (!newVersionResult.IsSuccess)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<SchemaVersionDto>.Failure(newVersionResult.Error ?? "Failed to create new schema version.");
            }

            // Mark change request as applied
            changeRequest.MarkAsApplied(userId);
            await _unitOfWork.SchemaChangeRequests.UpdateAsync(changeRequest, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await _auditService.LogAsync(
                AuditAction.SchemaChangeApplied,
                nameof(SchemaChangeRequest),
                changeRequest.Id,
                userId,
                userId,
                null,
                System.Text.Json.JsonSerializer.Serialize(MapToDto(changeRequest)),
                $"Applied and created new schema version: {newVersionResult.Value?.Id}",
                null,
                correlationId,
                cancellationToken);

            return newVersionResult;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result> RejectAsync(Guid id, string reason, string userId, CancellationToken cancellationToken = default)
    {
        var changeRequest = await _unitOfWork.SchemaChangeRequests.GetByIdAsync(id, cancellationToken);
        if (changeRequest == null)
            return Result.Failure("Change request not found.");

        var stateBefore = System.Text.Json.JsonSerializer.Serialize(MapToDto(changeRequest));

        changeRequest.Reject(reason, userId);
        await _unitOfWork.SchemaChangeRequests.UpdateAsync(changeRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.SchemaChangeRejected,
            nameof(SchemaChangeRequest),
            changeRequest.Id,
            userId,
            userId,
            stateBefore,
            System.Text.Json.JsonSerializer.Serialize(MapToDto(changeRequest)),
            $"Rejected. Reason: {reason}",
            null,
            null,
            cancellationToken);

        return Result.Success();
    }

    private static SchemaChangeRequestDto MapToDto(SchemaChangeRequest entity)
    {
        return new SchemaChangeRequestDto(
            entity.Id,
            entity.SchemaVersionId,
            entity.ChangeType,
            entity.ChangeDetails,
            entity.Description,
            entity.Justification,
            entity.ImpactAnalysis,
            entity.Status,
            entity.IsAiSuggested,
            entity.AiConfidenceScore,
            entity.IsDestructive,
            entity.Approvals.Select(a => new SchemaChangeApprovalDto(
                a.Id,
                a.Layer,
                a.IsApproved,
                a.ApproverUserId,
                a.ApproverName,
                a.Comments,
                a.ApprovedAt)).ToList(),
            entity.CreatedAt,
            entity.CreatedBy);
    }
}
