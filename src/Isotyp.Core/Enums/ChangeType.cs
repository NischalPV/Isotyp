namespace Isotyp.Core.Enums;

/// <summary>
/// Types of schema/model changes.
/// </summary>
public enum ChangeType
{
    /// <summary>
    /// New table/entity added.
    /// </summary>
    AddTable = 0,

    /// <summary>
    /// New column/field added (additive - preferred).
    /// </summary>
    AddColumn = 1,

    /// <summary>
    /// Column/field modified.
    /// </summary>
    ModifyColumn = 2,

    /// <summary>
    /// Table/entity removed (destructive - requires extra approval).
    /// </summary>
    RemoveTable = 3,

    /// <summary>
    /// Column/field removed (destructive - requires extra approval).
    /// </summary>
    RemoveColumn = 4,

    /// <summary>
    /// Index added.
    /// </summary>
    AddIndex = 5,

    /// <summary>
    /// Index removed.
    /// </summary>
    RemoveIndex = 6,

    /// <summary>
    /// Relationship/constraint added.
    /// </summary>
    AddRelationship = 7,

    /// <summary>
    /// Relationship/constraint modified.
    /// </summary>
    ModifyRelationship = 8,

    /// <summary>
    /// Relationship/constraint removed.
    /// </summary>
    RemoveRelationship = 9,

    /// <summary>
    /// Table/entity renamed.
    /// </summary>
    RenameTable = 10,

    /// <summary>
    /// Column/field renamed.
    /// </summary>
    RenameColumn = 11
}
