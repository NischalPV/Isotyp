namespace IsoTyp.EntityFrameworkCore.Entities;

/// <summary>
/// Represents an immutable audit trail record for entity operations.
/// Stores comprehensive information about who did what, when, and whether it succeeded.
/// </summary>
public sealed record class AuditTrail : Entity<Guid>
{
    /// <summary>
    /// Unique identifier for the audit record.
    /// </summary>
    public AuditTrail()
    {
        _Id = Guid.CreateVersion7();
    }

    /// <summary>
    /// The name of the entity type being audited (e.g., "User", "Product").
    /// </summary>
    public string EntityName { get; private set; } = string.Empty;

    /// <summary>
    /// The identifier of the entity being audited (stored as string for flexibility).
    /// </summary>
    public string EntityId { get; private set; } = string.Empty;

    /// <summary>
    /// The type of operation performed (Create, Update, Delete, etc.).
    /// </summary>
    public AuditOperation Operation { get; private set; } = AuditOperation.Other;

    /// <summary>
    /// The status/result of the operation (Success, Failed, etc.).
    /// </summary>
    public AuditStatus Status { get; private set; } = AuditStatus.Pending;

    /// <summary>
    /// Identifier of the user who performed the operation.
    /// </summary>
    public string PerformedBy { get; private set; } = string.Empty;

    /// <summary>
    /// Timestamp when the operation was performed (UTC).
    /// </summary>
    public DateTime PerformedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// IP address from which the operation was performed (if available).
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// User agent or client information (if available).
    /// </summary>
    public string? UserAgent { get; private set; }

    /// <summary>
    /// JSON representation of the entity's old values (before the operation).
    /// Null for Create operations.
    /// </summary>
    public string? OldValues { get; private set; }

    /// <summary>
    /// JSON representation of the entity's new values (after the operation).
    /// Null for Delete operations.
    /// </summary>
    public string? NewValues { get; private set; }

    /// <summary>
    /// JSON array of property names that were changed.
    /// </summary>
    public string? ChangedProperties { get; private set; }

    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Stack trace if the operation failed (for debugging).
    /// </summary>
    public string? StackTrace { get; private set; }

    /// <summary>
    /// Correlation ID for tracking related operations across services.
    /// </summary>
    public string? CorrelationId { get; private set; }

    /// <summary>
    /// Additional metadata as JSON (extensibility point).
    /// </summary>
    public string? Metadata { get; private set; }

    /// <summary>
    /// Database transaction ID if available.
    /// </summary>
    public string? TransactionId { get; internal set; }

    /// <summary>
    /// Duration of the operation in milliseconds (if tracked).
    /// </summary>
    public long? DurationMs { get; private set; }

    /// <summary>
    /// Creates a populated audit trail entry.
    /// </summary>
    public AuditTrail(
        string entityName,
        string entityId,
        AuditOperation operation,
        AuditStatus status,
        string performedBy,
        DateTime? performedAt = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? oldValues = null,
        string? newValues = null,
        string? changedProperties = null,
        string? errorMessage = null,
        string? stackTrace = null,
        string? correlationId = null,
        string? metadata = null,
        string? transactionId = null,
        long? durationMs = null)
        : this()
    {
        EntityName = entityName;
        EntityId = entityId;
        Operation = operation;
        Status = status;
        PerformedBy = performedBy;
        PerformedAt = performedAt ?? DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        OldValues = oldValues;
        NewValues = newValues;
        ChangedProperties = changedProperties;
        ErrorMessage = errorMessage;
        StackTrace = stackTrace;
        CorrelationId = correlationId;
        Metadata = metadata;
        TransactionId = transactionId;
        DurationMs = durationMs;
    }
}

/// <summary>
/// Defines the type of action being audited.
/// </summary>
public enum AuditOperation
{
    /// <summary>Entity creation.</summary>
    Create = 1,

    /// <summary>Entity update.</summary>
    Update,

    /// <summary>Entity deletion.</summary>
    Delete,

    /// <summary>Entity restoration.</summary>
    Restore,

    /// <summary>Custom or other operation.</summary>
    Other
}

/// <summary>
/// Represents the outcome of the audited action.
/// </summary>
public enum AuditStatus
{
    /// <summary>Operation succeeded.</summary>
    Success = 1,

    /// <summary>Operation failed.</summary>
    Failed,

    /// <summary>Operation is pending or in-progress.</summary>
    Pending
}
