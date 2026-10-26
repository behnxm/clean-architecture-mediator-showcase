using CleanArchitectureMediatorShowcase.Domain.Abstractions;
using CleanArchitectureMediatorShowcase.Domain.Common;
using CleanArchitectureMediatorShowcase.Domain.Errors;
using Mediator;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.DeleteProduct;

/// <summary>
/// Command handler for removing a product from the catalog.
/// </summary>
public sealed class DeleteProductCommandHandler(
    IProductRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteProductCommand, Result>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var product = await repository.GetByIdAsync(command.Id, cancellationToken).ConfigureAwait(false);
        if (product is null)
        {
            return ProductErrors.NotFound(command.Id);
        }

        repository.Remove(product);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
