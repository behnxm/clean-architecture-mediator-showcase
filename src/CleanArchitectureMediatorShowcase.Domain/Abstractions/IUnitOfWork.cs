namespace CleanArchitectureMediatorShowcase.Domain.Abstractions;

/// <summary>
/// Defines the contract for coordinating transactional changes across the domain model.
/// Demonstrates the Dependency Inversion Principle (DIP):
/// High-level application use cases depend on this abstraction defined in the domain layer,
/// while the low-level persistence implementation resides in the infrastructure layer (e.g. EF Core DbContext).
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all tracked aggregate changes atomically to the underlying store.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to abort the asynchronous save.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
