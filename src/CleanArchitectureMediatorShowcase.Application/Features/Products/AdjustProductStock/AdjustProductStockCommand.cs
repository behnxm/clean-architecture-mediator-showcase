using CleanArchitectureMediatorShowcase.Domain.Common;
using Mediator;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.AdjustProductStock;

/// <summary>
/// Command to adjust the stock inventory of a product by adding or subtracting an integer quantity.
/// </summary>
/// <param name="Id">Unique identifier of the target product.</param>
/// <param name="Delta">The positive or negative quantity to add to current stock.</param>
public sealed record AdjustProductStockCommand(Guid Id, int Delta) : ICommand<Result>;
