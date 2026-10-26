namespace CleanArchitectureMediatorShowcase.Domain.Common;

/// <summary>
/// Generic base class for domain entities providing identity, equality semantics based on ID,
/// and encapsulation of domain events.
/// </summary>
/// <typeparam name="TId">The type of entity identifier.</typeparam>
public abstract class BaseEntity<TId> : IEquatable<BaseEntity<TId>>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Initializes a new instance of <see cref="BaseEntity{TId}"/>.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    protected BaseEntity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Parameterless constructor required by ORM frameworks (e.g., EF Core).
    /// </summary>
    protected BaseEntity()
    {
        Id = default!;
    }

    /// <summary>
    /// Gets the unique identifier for this entity.
    /// </summary>
    public TId Id { get; protected init; }

    /// <summary>
    /// Gets the read-only collection of domain events raised by this entity instance.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the entity event queue.
    /// </summary>
    /// <param name="domainEvent">The domain event to append.</param>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all queued domain events after they have been published or committed.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <inheritdoc />
    public bool Equals(BaseEntity<TId>? other)
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
    public override bool Equals(object? obj)
    {
        return obj is BaseEntity<TId> other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return EqualityComparer<TId>.Default.GetHashCode(Id);
    }

    /// <summary>
    /// Equality operator overload comparing entities by identifier.
    /// </summary>
    public static bool operator ==(BaseEntity<TId>? left, BaseEntity<TId>? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Inequality operator overload comparing entities by identifier.
    /// </summary>
    public static bool operator !=(BaseEntity<TId>? left, BaseEntity<TId>? right)
    {
        return !Equals(left, right);
    }
}
