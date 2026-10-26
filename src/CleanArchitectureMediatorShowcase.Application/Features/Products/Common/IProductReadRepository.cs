using CleanArchitectureMediatorShowcase.Application.Common.Models;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.Common;

/// <summary>
/// Read-side query abstraction for retrieving product data directly without instantiating domain aggregates.
/// Demonstrates CQRS Query separation: queries bypass aggregate lifecycle rules and directly project SQL into DTOs
/// via Mapperly for performance and clarity.
/// </summary>
public interface IProductReadRepository
{
    /// <summary>
    /// Gets a single product DTO by identifier, bypassing domain tracking.
    /// </summary>
    /// <param name="id">The product unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The projected <see cref="ProductResponse"/> or null if not found.</returns>
    Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches products with server-side filtering, sorting, and pagination.
    /// </summary>
    /// <param name="criteria">Search and pagination criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of <see cref="ProductResponse"/> DTOs.</returns>
    Task<PagedList<ProductResponse>> SearchAsync(ProductSearchCriteria criteria, CancellationToken cancellationToken = default);
}
