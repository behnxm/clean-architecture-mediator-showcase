using CleanArchitectureMediatorShowcase.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.Common;

/// <summary>
/// Compile-time source-generated object mapper powered by Riok.Mapperly (Apache-2.0).
/// Generates deterministic, reflection-free mapping code compatible with Native AOT compilation.
/// </summary>
[Mapper]
public static partial class ProductMapper
{
    /// <summary>
    /// Maps an in-memory <see cref="Product"/> domain entity to a <see cref="ProductResponse"/> DTO.
    /// </summary>
    /// <param name="product">The domain product instance.</param>
    /// <returns>A mapped response DTO.</returns>
    [MapperIgnoreSource(nameof(Product.DomainEvents))]
    public static partial ProductResponse ToResponse(this Product product);

    /// <summary>
    /// Generates an expression tree projection for EF Core SQL queries, enabling database-level SELECT projection.
    /// </summary>
    /// <param name="query">The queryable product stream.</param>
    /// <returns>A projected queryable stream yielding <see cref="ProductResponse"/> DTOs.</returns>
    public static partial IQueryable<ProductResponse> ProjectToResponse(this IQueryable<Product> query);
}
