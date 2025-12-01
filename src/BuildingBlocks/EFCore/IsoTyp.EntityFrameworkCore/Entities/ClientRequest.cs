namespace IsoTyp.EntityFrameworkCore.Entities;

/// <summary>
/// Represents a client request used to guarantee idempotent processing.
/// </summary>
public sealed record class ClientRequest : Entity<Guid>
{
    /// <summary>
    /// Initializes a new client request with a generated identifier.
    /// </summary>
    public ClientRequest()
    {
        _Id = Guid.CreateVersion7();
    }

    /// <summary>
    /// Initializes a new client request with the provided values.
    /// </summary>
    /// <param name="name">The unique request name or key.</param>
    /// <param name="payload">Optional request payload for auditing/debugging.</param>
    /// <param name="owner">Optional owner or service initiating the request.</param>
    /// <param name="id">Optional identifier to support deterministic ids.</param>
    public ClientRequest(string name, string? payload = null, string? owner = null, Guid? id = null)
        : this()
    {
        _Id = id ?? _Id;
        Name = name;
        Payload = payload;
        Owner = owner;
    }

    /// <summary>
    /// Unique request name used for idempotency checks.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Timestamp when the request was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional payload capturing request parameters.
    /// </summary>
    public string? Payload { get; private set; }

    /// <summary>
    /// Optional identifier for the owner or system issuing the request.
    /// </summary>
    public string? Owner { get; private set; }
}
