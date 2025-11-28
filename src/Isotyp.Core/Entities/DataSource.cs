using Isotyp.Core.Enums;

namespace Isotyp.Core.Entities;

/// <summary>
/// Represents a data source configuration. Agents connect to these sources locally.
/// Metadata only is synchronized to cloud.
/// </summary>
public class DataSource : EntityBase
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DataSourceType Type { get; private set; }
    
    /// <summary>
    /// Connection string is stored locally by agents - never sent to cloud.
    /// This field stores a reference/placeholder only.
    /// </summary>
    public string ConnectionStringReference { get; private set; } = string.Empty;
    
    public bool IsActive { get; private set; }
    public DateTime? LastConnectedAt { get; private set; }
    public DateTime? LastValidatedAt { get; private set; }

    private DataSource() { }

    public static DataSource Create(
        string name,
        string description,
        DataSourceType type,
        string connectionStringReference,
        string createdBy)
    {
        var source = new DataSource
        {
            Name = name ?? throw new ArgumentNullException(nameof(name)),
            Description = description ?? string.Empty,
            Type = type,
            ConnectionStringReference = connectionStringReference ?? throw new ArgumentNullException(nameof(connectionStringReference)),
            IsActive = true
        };
        source.SetAuditFields(createdBy);
        return source;
    }

    public void Update(string name, string description, string updatedBy)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        UpdateAuditFields(updatedBy);
    }

    public void Deactivate(string updatedBy)
    {
        IsActive = false;
        UpdateAuditFields(updatedBy);
    }

    public void Activate(string updatedBy)
    {
        IsActive = true;
        UpdateAuditFields(updatedBy);
    }

    public void RecordConnection(DateTime connectedAt)
    {
        LastConnectedAt = connectedAt;
    }

    public void RecordValidation(DateTime validatedAt)
    {
        LastValidatedAt = validatedAt;
    }
}
