using CleanArchitectureMediatorShowcase.Domain.Common;
using Mediator;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.DeleteProduct;

/// <summary>
/// Command to delete a product from the catalog.
/// </summary>
/// <param name="Id">Unique identifier of the product to remove.</param>
public sealed record DeleteProductCommand(Guid Id) : ICommand<Result>;
