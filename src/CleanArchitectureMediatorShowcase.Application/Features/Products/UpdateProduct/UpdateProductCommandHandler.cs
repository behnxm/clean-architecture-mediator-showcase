using CleanArchitectureMediatorShowcase.Domain.Abstractions;
using CleanArchitectureMediatorShowcase.Domain.Common;
using CleanArchitectureMediatorShowcase.Domain.Errors;
using Mediator;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.UpdateProduct;

/// <summary>
/// Command handler for updating an existing product.
/// Verifies entity existence, checks for name collisions, applies updates, and commits changes.
/// </summary>
public sealed class UpdateProductCommandHandler(
    IProductRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateProductCommand, Result>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var product = await repository.GetByIdAsync(command.Id, cancellationToken).ConfigureAwait(false);
        if (product is null)
        {
            return ProductErrors.NotFound(command.Id);
        }

        var isDuplicate = await repository.ExistsByNameAsync(command.Name, command.Id, cancellationToken).ConfigureAwait(false);
        if (isDuplicate)
        {
            return ProductErrors.DuplicateName;
        }

        var updateResult = product.Update(command.Name, command.Description, command.Price);
        if (updateResult.IsFailure)
        {
            return updateResult.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
