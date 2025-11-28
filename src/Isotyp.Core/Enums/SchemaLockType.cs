namespace Isotyp.Core.Enums;

/// <summary>
/// Types of schema locks that can be applied.
/// </summary>
public enum SchemaLockType
{
    /// <summary>
    /// No lock - schema can be modified.
    /// </summary>
    None = 0,

    /// <summary>
    /// Soft lock - AI suggestions allowed, but no auto-apply.
    /// </summary>
    SoftLock = 1,

    /// <summary>
    /// Hard lock - No modifications allowed at all.
    /// </summary>
    HardLock = 2,

    /// <summary>
    /// Additive only - Only new fields/tables can be added.
    /// </summary>
    AdditiveOnly = 3
}
