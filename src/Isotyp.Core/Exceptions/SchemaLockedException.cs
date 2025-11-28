namespace Isotyp.Core.Exceptions;

/// <summary>
/// Exception thrown when a schema is locked and modifications are not allowed.
/// </summary>
public class SchemaLockedException : DomainException
{
    public Guid SchemaVersionId { get; }

    public SchemaLockedException(Guid schemaVersionId)
        : base($"Schema version {schemaVersionId} is locked and cannot be modified.")
    {
        SchemaVersionId = schemaVersionId;
    }

    public SchemaLockedException(Guid schemaVersionId, string message)
        : base(message)
    {
        SchemaVersionId = schemaVersionId;
    }
}
