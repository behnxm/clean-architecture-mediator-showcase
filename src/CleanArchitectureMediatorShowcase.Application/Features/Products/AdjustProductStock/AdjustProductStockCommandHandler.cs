using CleanArchitectureMediatorShowcase.Domain.Abstractions;
using CleanArchitectureMediatorShowcase.Domain.Common;
using CleanArchitectureMediatorShowcase.Domain.Errors;
using Mediator;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.AdjustProductStock;

/// <summary>
/// Command handler for mutating product inventory stock.
/// </summary>
public sealed class AdjustProductStockCommandHandler(
    IProductRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AdjustProductStockCommand, Result>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(AdjustProductStockCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var product = await repository.GetByIdAsync(command.Id, cancellationToken).ConfigureAwait(false);
        if (product is null)
        {
            return ProductErrors.NotFound(command.Id);
        }

        var adjustResult = product.AdjustStock(command.Delta);
        if (adjustResult.IsFailure)
        {
            return adjustResult.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
