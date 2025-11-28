namespace Isotyp.Core.Enums;

/// <summary>
/// Layers of approval required for schema changes.
/// </summary>
public enum ApprovalLayer
{
    /// <summary>
    /// Technical review - validates schema correctness.
    /// </summary>
    Technical = 0,

    /// <summary>
    /// Business review - validates business impact.
    /// </summary>
    Business = 1,

    /// <summary>
    /// Data governance review - validates compliance and security.
    /// </summary>
    DataGovernance = 2
}
