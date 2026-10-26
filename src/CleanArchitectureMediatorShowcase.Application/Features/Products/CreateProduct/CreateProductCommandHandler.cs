using CleanArchitectureMediatorShowcase.Domain.Abstractions;
using CleanArchitectureMediatorShowcase.Domain.Common;
using CleanArchitectureMediatorShowcase.Domain.Entities;
using CleanArchitectureMediatorShowcase.Domain.Errors;
using Mediator;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.CreateProduct;

/// <summary>
/// Command handler for creating a new product catalog item.
/// Checks for name uniqueness, invokes the domain factory method, and persists via unit of work.
/// </summary>
public sealed class CreateProductCommandHandler(
    IProductRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateProductCommand, Result<Guid>>
{
    /// <inheritdoc />
    public async ValueTask<Result<Guid>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var exists = await repository.ExistsByNameAsync(command.Name, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (exists)
        {
            return ProductErrors.DuplicateName;
        }

        var createResult = Product.Create(command.Name, command.Description, command.Price, command.Stock);
        if (createResult.IsFailure)
        {
            return createResult.Error;
        }

        var product = createResult.Value;
        await repository.AddAsync(product, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return product.Id;
    }
}
