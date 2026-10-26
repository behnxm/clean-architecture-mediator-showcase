using CleanArchitectureMediatorShowcase.Application.Features.Products.Common;
using CleanArchitectureMediatorShowcase.Domain.Common;
using CleanArchitectureMediatorShowcase.Domain.Errors;
using Mediator;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.GetProductById;

/// <summary>
/// Query handler for retrieving a single product by identifier via the read-side repository.
/// </summary>
public sealed class GetProductByIdQueryHandler(IProductReadRepository readRepository)
    : IQueryHandler<GetProductByIdQuery, Result<ProductResponse>>
{
    /// <inheritdoc />
    public async ValueTask<Result<ProductResponse>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var product = await readRepository.GetByIdAsync(query.Id, cancellationToken).ConfigureAwait(false);
        if (product is null)
        {
            return ProductErrors.NotFound(query.Id);
        }

        return product;
    }
}
