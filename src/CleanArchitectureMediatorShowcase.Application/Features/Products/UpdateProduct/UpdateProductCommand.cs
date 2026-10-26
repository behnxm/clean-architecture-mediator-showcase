using CleanArchitectureMediatorShowcase.Domain.Common;
using Mediator;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.UpdateProduct;

/// <summary>
/// Command to update the details of an existing product.
/// </summary>
/// <param name="Id">Identifier of the product to update.</param>
/// <param name="Name">Updated product name.</param>
/// <param name="Description">Updated product description.</param>
/// <param name="Price">Updated unit price.</param>
public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price) : ICommand<Result>;
