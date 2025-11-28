namespace Isotyp.Core.Exceptions;

/// <summary>
/// Exception thrown when rollback operation fails.
/// </summary>
public class RollbackException : DomainException
{
    public Guid SchemaVersionId { get; }

    public RollbackException(Guid schemaVersionId, string message)
        : base(message)
    {
        SchemaVersionId = schemaVersionId;
    }

    public RollbackException(Guid schemaVersionId, string message, Exception innerException)
        : base(message, innerException)
    {
        SchemaVersionId = schemaVersionId;
    }
}
