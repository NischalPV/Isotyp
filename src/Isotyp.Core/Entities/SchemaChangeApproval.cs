using Isotyp.Core.Enums;

namespace Isotyp.Core.Entities;

/// <summary>
/// Represents an approval decision for a schema change at a specific approval layer.
/// </summary>
public class SchemaChangeApproval : EntityBase
{
    public Guid SchemaChangeRequestId { get; private set; }
    public ApprovalLayer Layer { get; private set; }
    public bool IsApproved { get; private set; }
    public string ApproverUserId { get; private set; } = string.Empty;
    public string ApproverName { get; private set; } = string.Empty;
    public string Comments { get; private set; } = string.Empty;
    public DateTime ApprovedAt { get; private set; }

    private SchemaChangeApproval() { }

    public static SchemaChangeApproval Create(
        Guid schemaChangeRequestId,
        ApprovalLayer layer,
        bool isApproved,
        string approverUserId,
        string approverName,
        string comments,
        string createdBy)
    {
        var approval = new SchemaChangeApproval
        {
            SchemaChangeRequestId = schemaChangeRequestId,
            Layer = layer,
            IsApproved = isApproved,
            ApproverUserId = approverUserId ?? throw new ArgumentNullException(nameof(approverUserId)),
            ApproverName = approverName ?? throw new ArgumentNullException(nameof(approverName)),
            Comments = comments ?? string.Empty,
            ApprovedAt = DateTime.UtcNow
        };
        approval.SetAuditFields(createdBy);
        return approval;
    }
}
