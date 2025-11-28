namespace Isotyp.Core.Enums;

/// <summary>
/// Represents the approval status of schema changes and other modifications.
/// </summary>
public enum ApprovalStatus
{
    /// <summary>
    /// Change is pending initial review.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Change has been submitted for approval.
    /// </summary>
    Submitted = 1,

    /// <summary>
    /// First layer (Technical) approval completed.
    /// </summary>
    TechnicalApproved = 2,

    /// <summary>
    /// Second layer (Business) approval completed.
    /// </summary>
    BusinessApproved = 3,

    /// <summary>
    /// Final approval by data governance/admin.
    /// </summary>
    FullyApproved = 4,

    /// <summary>
    /// Change was rejected at any layer.
    /// </summary>
    Rejected = 5,

    /// <summary>
    /// Change was applied successfully.
    /// </summary>
    Applied = 6,

    /// <summary>
    /// Change was rolled back.
    /// </summary>
    RolledBack = 7
}
