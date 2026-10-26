namespace CleanArchitectureMediatorShowcase.Domain.Common;

/// <summary>
/// Defines audit tracking metadata for domain entities.
/// In Clean Architecture, domain entities expose audit properties as read-only getters,
/// while the infrastructure layer (e.g., EF Core interceptor) manages mutation via reflection or backing property access.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Gets the date and time when the entity was created in UTC.
    /// </summary>
    DateTime CreatedAtUtc { get; }

    /// <summary>
    /// Gets the date and time when the entity was last modified in UTC, or null if unmodified.
    /// </summary>
    DateTime? LastModifiedAtUtc { get; }
}
