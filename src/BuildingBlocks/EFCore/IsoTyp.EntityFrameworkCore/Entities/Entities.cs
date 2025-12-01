namespace IsoTyp.EntityFrameworkCore.Entities;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using MediatR;

/// <summary>
/// Represents the base interface for all domain entities.
/// </summary>
public record IBaseEntity
{
    private readonly ConcurrentQueue<INotification> _domainEvents = [];

    /// <summary>
    /// Domain events associated with this entity.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyCollection<INotification> DomainEvents => [.. _domainEvents];

    /// <summary>
    /// Indicates whether this entity has any pending domain events.
    /// </summary>
    [JsonIgnore]
    public bool HasDomainEvents => !_domainEvents.IsEmpty;

    /// <summary>
    /// Adds a domain event to the queue for later dispatch.
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    protected void RaiseDomainEvent(INotification domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Enqueue(domainEvent);
    }

    /// <summary>
    /// Removes the specified domain event if it exists.
    /// </summary>
    /// <param name="domainEvent">The event to remove.</param>
    /// <returns><c>true</c> if the event was removed; otherwise, <c>false</c>.</returns>
    public bool RemoveDomainEvent(INotification domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var events = new List<INotification>();
        var removed = false;

        while (_domainEvents.TryDequeue(out var evt))
        {
            if (!removed && ReferenceEquals(evt, domainEvent))
            {
                removed = true;
                continue;
            }

            events.Add(evt);
        }

        foreach (var evt in events)
        {
            _domainEvents.Enqueue(evt);
        }

        return removed;
    }

    /// <summary>
    /// Clears all domain events from the queue.
    /// </summary>
    public void ClearDomainEvents()
    {
        while (_domainEvents.TryDequeue(out _))
        {
        }
    }

    /// <summary>
    /// Indicates whether the entity is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Base entity class providing identity and domain event capabilities.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract record class Entity<TId> : IBaseEntity, IEquatable<Entity<TId>>
{
    /// <summary>
    /// Backing field for the entity identifier.
    /// </summary>
    public TId _Id { get; protected set; } = default!;

    /// <summary>
    /// Gets or sets the entity identifier.
    /// </summary>
    public virtual TId Id
    {
        get => _Id;
        protected set => _Id = value;
    }

    #region Equality - Proper entity equality based on ID and type

    /// <inheritdoc />
    public virtual bool Equals(Entity<TId>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <inheritdoc />
    public override int GetHashCode() =>
        Id is null ? 0 : EqualityComparer<TId>.Default.GetHashCode(Id);

    #endregion
}

/// <summary>
/// Represents the base aggregate root providing versioning support.
/// </summary>
/// <typeparam name="TId">The aggregate identifier type.</typeparam>
public abstract record class AggregateRoot<TId> : Entity<TId>
{
    /// <summary>
    /// Version for optimistic concurrency control.
    /// </summary>
    public virtual uint Revision { get; protected set; } = 1;

    /// <summary>
    /// Increments the revision for a successful change.
    /// </summary>
    protected void IncrementRevision() => Revision++;
}

/// <summary>
/// Represents an aggregate root with auditing and semantic versioning support.
/// </summary>
/// <typeparam name="TId">The entity identifier type.</typeparam>
public abstract record class AuditableEntity<TId> : AggregateRoot<TId>
{
    private string _version = "1.0.0";

    /// <summary>
    /// Timestamp when the entity was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Identifier of the user who created the entity.
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;

    /// <summary>
    /// Timestamp when the entity was last modified (UTC).
    /// </summary>
    public DateTime? ModifiedAt { get; private set; }

    /// <summary>
    /// Identifier of the user who last modified the entity.
    /// </summary>
    public string? ModifiedBy { get; private set; }

    /// <summary>
    /// Semantic version string following MAJOR.MINOR.PATCH.
    /// </summary>
    public string Version
    {
        get => _version;
        private set => _version = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Marks the entity as created by a specific user.
    /// </summary>
    /// <param name="by">The creator identifier.</param>
    /// <param name="at">Optional creation timestamp.</param>
    public void MarkCreated(string by, DateTime? at = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(by);
        CreatedBy = by;
        CreatedAt = at ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the entity as modified by a specific user.
    /// </summary>
    /// <param name="by">The modifier identifier.</param>
    /// <param name="at">Optional modification timestamp.</param>
    public void MarkModified(string by, DateTime? at = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(by);
        ModifiedBy = by;
        ModifiedAt = at ?? DateTime.UtcNow;
        IncrementSemanticVersion();
        IncrementRevision();
    }

    /// <summary>
    /// Marks the entity as deleted and raises a deletion domain event.
    /// </summary>
    /// <param name="by">The user performing the deletion.</param>
    /// <param name="at">Optional timestamp of deletion.</param>
    public void Delete(string by, DateTime? at = null)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        MarkModified(by, at);
        RaiseDomainEvent(new EntityDeleted<TId>(Id, by, at ?? DateTime.UtcNow));
    }

    /// <summary>
    /// Restores a previously deleted entity and raises a restoration domain event.
    /// </summary>
    /// <param name="by">The user performing the restoration.</param>
    /// <param name="at">Optional timestamp of restoration.</param>
    public void Restore(string by, DateTime? at = null)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        MarkModified(by, at);
        RaiseDomainEvent(new EntityRestored<TId>(Id, by, at ?? DateTime.UtcNow));
    }

    /// <summary>
    /// Sets the semantic version explicitly after validation.
    /// </summary>
    /// <param name="version">The version string.</param>
    public void SetVersion(string version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        if (!IsValidSemanticVersion(version))
        {
            throw new ArgumentException($"Invalid semantic version format: {version}", nameof(version));
        }

        Version = version;
    }

    /// <summary>
    /// Increments the PATCH component of the semantic version.
    /// </summary>
    private void IncrementSemanticVersion()
    {
        var parts = ParseSemanticVersion(Version);
        parts.patch++;
        Version = FormatSemanticVersion(parts);
    }

    /// <summary>
    /// Increments the MINOR component of the semantic version.
    /// </summary>
    public void IncrementMinorVersion()
    {
        var parts = ParseSemanticVersion(Version);
        parts.minor++;
        parts.patch = 0;
        Version = FormatSemanticVersion(parts);
    }

    /// <summary>
    /// Increments the MAJOR component of the semantic version.
    /// </summary>
    public void IncrementMajorVersion()
    {
        var parts = ParseSemanticVersion(Version);
        parts.major++;
        parts.minor = 0;
        parts.patch = 0;
        Version = FormatSemanticVersion(parts);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidSemanticVersion(string version)
    {
        var parts = version.Split('.');
        return parts.Length >= 1 && parts.Length <= 3 &&
               parts.All(p => int.TryParse(p, out var num) && num >= 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (int major, int minor, int patch) ParseSemanticVersion(string version)
    {
        var parts = version.Split('.').Select(int.Parse).ToArray();
        return parts.Length switch
        {
            1 => (parts[0], 0, 0),
            2 => (parts[0], parts[1], 0),
            _ => (parts[0], parts[1], parts[2])
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string FormatSemanticVersion((int major, int minor, int patch) parts) =>
        $"{parts.major}.{parts.minor}.{parts.patch}";
}

/// <summary>
/// Represents a value object with component-based equality.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Components used to compare two value objects for equality.
    /// </summary>
    /// <returns>The components participating in equality.</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc />
    public virtual bool Equals(ValueObject? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is ValueObject valueObject && Equals(valueObject);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var component in GetEqualityComponents())
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }

    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right) =>
        !Equals(left, right);
}

#region Domain Events

/// <summary>
/// Domain event raised when an entity is deleted.
/// </summary>
/// <param name="EntityId">Identifier of the deleted entity.</param>
/// <param name="DeletedBy">User who performed the deletion.</param>
/// <param name="DeletedAt">Timestamp of deletion.</param>
public sealed record EntityDeleted<TId>(TId EntityId, string DeletedBy, DateTime DeletedAt) : INotification
{
    /// <summary>
    /// When the event occurred (UTC).
    /// </summary>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Unique event identifier.
    /// </summary>
    public Guid Id { get; } = Guid.CreateVersion7();

    /// <summary>
    /// Correlation id for tracing related operations.
    /// </summary>
    public string CorrelationId { get; set; } = EntityId?.ToString() ?? Guid.NewGuid().ToString();
}

/// <summary>
/// Domain event raised when an entity is restored.
/// </summary>
/// <param name="EntityId">Identifier of the restored entity.</param>
/// <param name="RestoredBy">User who performed the restoration.</param>
/// <param name="RestoredAt">Timestamp of restoration.</param>
public sealed record EntityRestored<TId>(TId EntityId, string RestoredBy, DateTime RestoredAt) : INotification
{
    /// <summary>
    /// When the event occurred (UTC).
    /// </summary>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Unique event identifier.
    /// </summary>
    public Guid Id { get; } = Guid.CreateVersion7();

    /// <summary>
    /// Correlation id for tracing related operations.
    /// </summary>
    public string CorrelationId { get; set; } = EntityId?.ToString() ?? Guid.NewGuid().ToString();
}

#endregion
