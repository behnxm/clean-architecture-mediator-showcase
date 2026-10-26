using CleanArchitectureMediatorShowcase.Domain.Entities;

namespace CleanArchitectureMediatorShowcase.Domain.Abstractions;

/// <summary>
/// Defines write-side persistence operations for the <see cref="Product"/> aggregate root.
/// Demonstrates the Dependency Inversion Principle (DIP) and Repository Pattern:
/// The domain layer dictates the interface needed to mutate the domain aggregate,
/// while the infrastructure layer provides the concrete database implementation (e.g. EF Core DbSet).
/// Queries that bypass the aggregate root for optimized projection use a separate read repository abstraction.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Retrieves a product by identifier with EF tracking enabled for updates.
    /// </summary>
    /// <param name="id">The unique identifier of the product aggregate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tracked <see cref="Product"/> aggregate, or null if not found.</returns>
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a newly created product aggregate to the persistence context.
    /// </summary>
    /// <param name="product">The product aggregate root.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing product aggregate for deletion.
    /// </summary>
    /// <param name="product">The product aggregate root to remove.</param>
    void Remove(Product product);

    /// <summary>
    /// Checks whether another product exists with the given name, optionally excluding a specific product id (used during updates).
    /// </summary>
    /// <param name="name">The name to check for duplicates.</param>
    /// <param name="excludeId">Optional ID of the current entity being updated to exclude from comparison.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if a conflicting product name exists; otherwise false.</returns>
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
