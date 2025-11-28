using Isotyp.Core.Enums;

namespace Isotyp.Core.Entities;

/// <summary>
/// Represents a proposed change to a schema. AI may suggest changes, but they are never auto-applied.
/// All changes require explicit multi-layer human approval.
/// </summary>
public class SchemaChangeRequest : EntityBase
{
    public Guid SchemaVersionId { get; private set; }
    public ChangeType ChangeType { get; private set; }
    
    /// <summary>
    /// JSON representation of the proposed change details.
    /// </summary>
    public string ChangeDetails { get; private set; } = string.Empty;
    
    /// <summary>
    /// Human-readable description of the change.
    /// </summary>
    public string Description { get; private set; } = string.Empty;
    
    /// <summary>
    /// Justification for the change (required for approval).
    /// </summary>
    public string Justification { get; private set; } = string.Empty;
    
    /// <summary>
    /// Impact analysis of the change.
    /// </summary>
    public string ImpactAnalysis { get; private set; } = string.Empty;
    
    public ApprovalStatus Status { get; private set; }
    
    /// <summary>
    /// Whether this change was suggested by AI.
    /// </summary>
    public bool IsAiSuggested { get; private set; }
    
    /// <summary>
    /// AI confidence score (0-100) if AI suggested.
    /// </summary>
    public int? AiConfidenceScore { get; private set; }
    
    /// <summary>
    /// Whether this is a destructive change (requires extra approval).
    /// </summary>
    public bool IsDestructive { get; private set; }

    private readonly List<SchemaChangeApproval> _approvals = new();
    public IReadOnlyCollection<SchemaChangeApproval> Approvals => _approvals.AsReadOnly();

    private SchemaChangeRequest() { }

    public static SchemaChangeRequest Create(
        Guid schemaVersionId,
        ChangeType changeType,
        string changeDetails,
        string description,
        string justification,
        string impactAnalysis,
        bool isAiSuggested,
        int? aiConfidenceScore,
        string createdBy)
    {
        var request = new SchemaChangeRequest
        {
            SchemaVersionId = schemaVersionId,
            ChangeType = changeType,
            ChangeDetails = changeDetails ?? throw new ArgumentNullException(nameof(changeDetails)),
            Description = description ?? throw new ArgumentNullException(nameof(description)),
            Justification = justification ?? string.Empty,
            ImpactAnalysis = impactAnalysis ?? string.Empty,
            Status = ApprovalStatus.Pending,
            IsAiSuggested = isAiSuggested,
            AiConfidenceScore = aiConfidenceScore,
            IsDestructive = IsDestructiveChange(changeType)
        };
        request.SetAuditFields(createdBy);
        return request;
    }

    private static bool IsDestructiveChange(ChangeType changeType)
    {
        return changeType == ChangeType.RemoveTable ||
               changeType == ChangeType.RemoveColumn ||
               changeType == ChangeType.RemoveIndex ||
               changeType == ChangeType.RemoveRelationship ||
               changeType == ChangeType.ModifyColumn;
    }

    public void Submit(string justification, string submittedBy)
    {
        if (Status != ApprovalStatus.Pending)
            throw new InvalidOperationException("Can only submit pending change requests.");
        
        Justification = justification ?? throw new ArgumentNullException(nameof(justification));
        Status = ApprovalStatus.Submitted;
        UpdateAuditFields(submittedBy);
    }

    public void AddApproval(SchemaChangeApproval approval)
    {
        if (approval == null)
            throw new ArgumentNullException(nameof(approval));
        
        // Check if this layer has already approved/rejected
        if (_approvals.Any(a => a.Layer == approval.Layer && !a.IsDeleted))
            throw new InvalidOperationException($"Approval for layer {approval.Layer} already exists.");
        
        _approvals.Add(approval);
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        var approvals = _approvals.Where(a => !a.IsDeleted).ToList();
        
        // Check if any rejection
        if (approvals.Any(a => !a.IsApproved))
        {
            Status = ApprovalStatus.Rejected;
            return;
        }

        // Count approvals
        var technicalApproved = approvals.Any(a => a.Layer == ApprovalLayer.Technical && a.IsApproved);
        var businessApproved = approvals.Any(a => a.Layer == ApprovalLayer.Business && a.IsApproved);
        var governanceApproved = approvals.Any(a => a.Layer == ApprovalLayer.DataGovernance && a.IsApproved);

        if (governanceApproved && businessApproved && technicalApproved)
            Status = ApprovalStatus.FullyApproved;
        else if (businessApproved && technicalApproved)
            Status = ApprovalStatus.BusinessApproved;
        else if (technicalApproved)
            Status = ApprovalStatus.TechnicalApproved;
    }

    public void MarkAsApplied(string appliedBy)
    {
        if (Status != ApprovalStatus.FullyApproved)
            throw new InvalidOperationException("Can only apply fully approved change requests.");
        
        Status = ApprovalStatus.Applied;
        UpdateAuditFields(appliedBy);
    }

    public void Reject(string reason, string rejectedBy)
    {
        if (Status == ApprovalStatus.Applied || Status == ApprovalStatus.RolledBack)
            throw new InvalidOperationException("Cannot reject applied or rolled back change requests.");
        
        Status = ApprovalStatus.Rejected;
        UpdateAuditFields(rejectedBy);
    }

    public bool IsReadyForApproval()
    {
        return Status == ApprovalStatus.Submitted && !string.IsNullOrWhiteSpace(Justification);
    }

    public bool RequiresExtraApproval()
    {
        return IsDestructive || (IsAiSuggested && AiConfidenceScore < 80);
    }
}
