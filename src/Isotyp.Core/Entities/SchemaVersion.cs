using Isotyp.Core.Enums;

namespace Isotyp.Core.Entities;

/// <summary>
/// Represents a versioned schema definition that forms the canonical understanding of data.
/// Schema versions are immutable once approved - new versions are created for changes.
/// </summary>
public class SchemaVersion : EntityBase
{
    public Guid DataSourceId { get; private set; }
    public int MajorVersion { get; private set; }
    public int MinorVersion { get; private set; }
    public int PatchVersion { get; private set; }
    
    /// <summary>
    /// JSON representation of the schema structure.
    /// </summary>
    public string SchemaDefinition { get; private set; } = string.Empty;
    
    /// <summary>
    /// Human-readable description of changes in this version.
    /// </summary>
    public string ChangeDescription { get; private set; } = string.Empty;
    
    public ApprovalStatus Status { get; private set; }
    public SchemaLockType LockType { get; private set; }
    public Guid? ParentVersionId { get; private set; }
    
    /// <summary>
    /// JSON representation of generated ORM mappings.
    /// </summary>
    public string OrmMappings { get; private set; } = string.Empty;
    
    /// <summary>
    /// SQL migration script to apply this schema version.
    /// </summary>
    public string MigrationScript { get; private set; } = string.Empty;
    
    /// <summary>
    /// SQL script to rollback this schema version.
    /// </summary>
    public string RollbackScript { get; private set; } = string.Empty;
    
    public DateTime? AppliedAt { get; private set; }
    public DateTime? RolledBackAt { get; private set; }
    public string? AppliedBy { get; private set; }
    public string? RolledBackBy { get; private set; }

    private SchemaVersion() { }

    public static SchemaVersion Create(
        Guid dataSourceId,
        int majorVersion,
        int minorVersion,
        int patchVersion,
        string schemaDefinition,
        string changeDescription,
        string ormMappings,
        string migrationScript,
        string rollbackScript,
        Guid? parentVersionId,
        string createdBy)
    {
        var version = new SchemaVersion
        {
            DataSourceId = dataSourceId,
            MajorVersion = majorVersion,
            MinorVersion = minorVersion,
            PatchVersion = patchVersion,
            SchemaDefinition = schemaDefinition ?? throw new ArgumentNullException(nameof(schemaDefinition)),
            ChangeDescription = changeDescription ?? string.Empty,
            OrmMappings = ormMappings ?? string.Empty,
            MigrationScript = migrationScript ?? string.Empty,
            RollbackScript = rollbackScript ?? string.Empty,
            ParentVersionId = parentVersionId,
            Status = ApprovalStatus.Pending,
            LockType = SchemaLockType.None
        };
        version.SetAuditFields(createdBy);
        return version;
    }

    public string GetVersionString() => $"{MajorVersion}.{MinorVersion}.{PatchVersion}";

    public void Submit(string submittedBy)
    {
        if (Status != ApprovalStatus.Pending)
            throw new InvalidOperationException("Can only submit pending schema versions.");
        
        Status = ApprovalStatus.Submitted;
        UpdateAuditFields(submittedBy);
    }

    public void ApproveTechnical(string approvedBy)
    {
        if (Status != ApprovalStatus.Submitted)
            throw new InvalidOperationException("Can only technically approve submitted schema versions.");
        
        Status = ApprovalStatus.TechnicalApproved;
        UpdateAuditFields(approvedBy);
    }

    public void ApproveBusiness(string approvedBy)
    {
        if (Status != ApprovalStatus.TechnicalApproved)
            throw new InvalidOperationException("Can only business approve technically approved schema versions.");
        
        Status = ApprovalStatus.BusinessApproved;
        UpdateAuditFields(approvedBy);
    }

    public void ApproveGovernance(string approvedBy)
    {
        if (Status != ApprovalStatus.BusinessApproved)
            throw new InvalidOperationException("Can only governance approve business approved schema versions.");
        
        Status = ApprovalStatus.FullyApproved;
        UpdateAuditFields(approvedBy);
    }

    public void Reject(string rejectedBy)
    {
        if (Status == ApprovalStatus.Applied || Status == ApprovalStatus.RolledBack)
            throw new InvalidOperationException("Cannot reject applied or rolled back schema versions.");
        
        Status = ApprovalStatus.Rejected;
        UpdateAuditFields(rejectedBy);
    }

    public void MarkAsApplied(string appliedBy, DateTime appliedAt)
    {
        if (Status != ApprovalStatus.FullyApproved)
            throw new InvalidOperationException("Can only apply fully approved schema versions.");
        
        Status = ApprovalStatus.Applied;
        AppliedAt = appliedAt;
        AppliedBy = appliedBy;
        UpdateAuditFields(appliedBy);
    }

    public void MarkAsRolledBack(string rolledBackBy, DateTime rolledBackAt)
    {
        if (Status != ApprovalStatus.Applied)
            throw new InvalidOperationException("Can only rollback applied schema versions.");
        
        Status = ApprovalStatus.RolledBack;
        RolledBackAt = rolledBackAt;
        RolledBackBy = rolledBackBy;
        UpdateAuditFields(rolledBackBy);
    }

    public void ApplyLock(SchemaLockType lockType, string lockedBy)
    {
        if (Status != ApprovalStatus.Applied)
            throw new InvalidOperationException("Can only lock applied schema versions.");
        
        LockType = lockType;
        UpdateAuditFields(lockedBy);
    }

    public void RemoveLock(string unlockedBy)
    {
        LockType = SchemaLockType.None;
        UpdateAuditFields(unlockedBy);
    }

    public bool CanBeModified()
    {
        return LockType == SchemaLockType.None || LockType == SchemaLockType.SoftLock;
    }

    public bool AllowsOnlyAdditive()
    {
        return LockType == SchemaLockType.AdditiveOnly;
    }

    public bool IsFullyLocked()
    {
        return LockType == SchemaLockType.HardLock;
    }
}
