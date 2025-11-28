namespace Isotyp.Core.Enums;

/// <summary>
/// Status of data validation against schema.
/// </summary>
public enum ValidationStatus
{
    /// <summary>
    /// Validation pending.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Validation in progress.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Validation passed.
    /// </summary>
    Passed = 2,

    /// <summary>
    /// Validation failed.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Validation passed with warnings.
    /// </summary>
    PassedWithWarnings = 4
}
