using CleanArchitectureMediatorShowcase.Application.Features.Products.Common;
using CleanArchitectureMediatorShowcase.Domain.Common;
using Mediator;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.GetProductById;

/// <summary>
/// Query to retrieve a single product by its unique identifier.
/// Bypasses domain aggregates and returns read-optimized <see cref="ProductResponse"/>.
/// </summary>
/// <param name="Id">The unique product identifier.</param>
public sealed record GetProductByIdQuery(Guid Id) : IQuery<Result<ProductResponse>>;
