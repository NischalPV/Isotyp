namespace Isotyp.Core.Entities;

/// <summary>
/// Base class for all domain entities providing common audit fields.
/// </summary>
public abstract class EntityBase
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public string CreatedBy { get; protected set; } = string.Empty;
    public string? UpdatedBy { get; protected set; }
    public int Version { get; protected set; } = 1;
    public bool IsDeleted { get; protected set; }

    protected void SetAuditFields(string createdBy)
    {
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        CreatedAt = DateTime.UtcNow;
    }

    protected void UpdateAuditFields(string updatedBy)
    {
        UpdatedBy = updatedBy ?? throw new ArgumentNullException(nameof(updatedBy));
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }

    public void MarkAsDeleted(string deletedBy)
    {
        IsDeleted = true;
        UpdateAuditFields(deletedBy);
    }
}
