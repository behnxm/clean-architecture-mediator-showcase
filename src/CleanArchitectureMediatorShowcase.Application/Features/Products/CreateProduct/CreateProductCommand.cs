using CleanArchitectureMediatorShowcase.Domain.Common;
using Mediator;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.CreateProduct;

/// <summary>
/// Command to create a new product entry in the catalog.
/// Implements Mediator's <see cref="ICommand{TResponse}"/> yielding <see cref="Result{Guid}"/>.
/// </summary>
/// <param name="Name">Product title.</param>
/// <param name="Description">Product description.</param>
/// <param name="Price">Unit price.</param>
/// <param name="Stock">Initial inventory count.</param>
public sealed record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int Stock) : ICommand<Result<Guid>>;
