using Isotyp.Core.Enums;

namespace Isotyp.Core.Entities;

/// <summary>
/// Represents an immutable audit log entry. All actions in the system are tracked.
/// </summary>
public class AuditLog : EntityBase
{
    /// <summary>
    /// Type of action performed.
    /// </summary>
    public AuditAction Action { get; private set; }
    
    /// <summary>
    /// Type of entity affected.
    /// </summary>
    public string EntityType { get; private set; } = string.Empty;
    
    /// <summary>
    /// ID of the entity affected.
    /// </summary>
    public Guid EntityId { get; private set; }
    
    /// <summary>
    /// User who performed the action.
    /// </summary>
    public string UserId { get; private set; } = string.Empty;
    
    /// <summary>
    /// User's display name.
    /// </summary>
    public string UserName { get; private set; } = string.Empty;
    
    /// <summary>
    /// JSON representation of the state before the change.
    /// </summary>
    public string? StateBefore { get; private set; }
    
    /// <summary>
    /// JSON representation of the state after the change.
    /// </summary>
    public string? StateAfter { get; private set; }
    
    /// <summary>
    /// Additional context/metadata about the action.
    /// </summary>
    public string? AdditionalData { get; private set; }
    
    /// <summary>
    /// IP address or identifier of the client/agent.
    /// </summary>
    public string? ClientIdentifier { get; private set; }
    
    /// <summary>
    /// Correlation ID for tracking related operations.
    /// </summary>
    public Guid? CorrelationId { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        AuditAction action,
        string entityType,
        Guid entityId,
        string userId,
        string userName,
        string? stateBefore,
        string? stateAfter,
        string? additionalData,
        string? clientIdentifier,
        Guid? correlationId)
    {
        var log = new AuditLog
        {
            Action = action,
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType)),
            EntityId = entityId,
            UserId = userId ?? throw new ArgumentNullException(nameof(userId)),
            UserName = userName ?? string.Empty,
            StateBefore = stateBefore,
            StateAfter = stateAfter,
            AdditionalData = additionalData,
            ClientIdentifier = clientIdentifier,
            CorrelationId = correlationId
        };
        log.SetAuditFields(userId);
        return log;
    }
}
