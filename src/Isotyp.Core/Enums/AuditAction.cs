namespace Isotyp.Core.Enums;

/// <summary>
/// Types of auditable actions in the system.
/// </summary>
public enum AuditAction
{
    /// <summary>
    /// Entity created.
    /// </summary>
    Create = 0,

    /// <summary>
    /// Entity updated.
    /// </summary>
    Update = 1,

    /// <summary>
    /// Entity deleted.
    /// </summary>
    Delete = 2,

    /// <summary>
    /// Schema change proposed.
    /// </summary>
    SchemaChangeProposed = 3,

    /// <summary>
    /// Schema change approved.
    /// </summary>
    SchemaChangeApproved = 4,

    /// <summary>
    /// Schema change rejected.
    /// </summary>
    SchemaChangeRejected = 5,

    /// <summary>
    /// Schema change applied.
    /// </summary>
    SchemaChangeApplied = 6,

    /// <summary>
    /// Schema change rolled back.
    /// </summary>
    SchemaChangeRolledBack = 7,

    /// <summary>
    /// AI suggestion generated.
    /// </summary>
    AiSuggestionGenerated = 8,

    /// <summary>
    /// Schema lock applied.
    /// </summary>
    SchemaLockApplied = 9,

    /// <summary>
    /// Schema lock removed.
    /// </summary>
    SchemaLockRemoved = 10,

    /// <summary>
    /// Data validation executed.
    /// </summary>
    DataValidationExecuted = 11,

    /// <summary>
    /// Agent connected.
    /// </summary>
    AgentConnected = 12,

    /// <summary>
    /// Agent disconnected.
    /// </summary>
    AgentDisconnected = 13
}
