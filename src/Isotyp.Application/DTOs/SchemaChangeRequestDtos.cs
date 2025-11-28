using Isotyp.Core.Enums;

namespace Isotyp.Application.DTOs;

/// <summary>
/// DTO for creating a schema change request.
/// </summary>
public record CreateSchemaChangeRequestDto(
    Guid SchemaVersionId,
    ChangeType ChangeType,
    string ChangeDetails,
    string Description,
    string ImpactAnalysis,
    bool IsAiSuggested = false,
    int? AiConfidenceScore = null);

/// <summary>
/// DTO for submitting a change request for approval.
/// </summary>
public record SubmitChangeRequestDto(
    Guid ChangeRequestId,
    string Justification);

/// <summary>
/// DTO for approving or rejecting a change request.
/// </summary>
public record ApproveChangeRequestDto(
    Guid ChangeRequestId,
    ApprovalLayer Layer,
    bool IsApproved,
    string ApproverUserId,
    string ApproverName,
    string Comments);

/// <summary>
/// DTO for schema change request response.
/// </summary>
public record SchemaChangeRequestDto(
    Guid Id,
    Guid SchemaVersionId,
    ChangeType ChangeType,
    string ChangeDetails,
    string Description,
    string Justification,
    string ImpactAnalysis,
    ApprovalStatus Status,
    bool IsAiSuggested,
    int? AiConfidenceScore,
    bool IsDestructive,
    IReadOnlyList<SchemaChangeApprovalDto> Approvals,
    DateTime CreatedAt,
    string CreatedBy);

/// <summary>
/// DTO for schema change approval response.
/// </summary>
public record SchemaChangeApprovalDto(
    Guid Id,
    ApprovalLayer Layer,
    bool IsApproved,
    string ApproverUserId,
    string ApproverName,
    string Comments,
    DateTime ApprovedAt);
