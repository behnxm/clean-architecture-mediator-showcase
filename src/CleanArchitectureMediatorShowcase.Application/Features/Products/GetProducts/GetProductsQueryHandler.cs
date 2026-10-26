using CleanArchitectureMediatorShowcase.Application.Common.Models;
using CleanArchitectureMediatorShowcase.Application.Features.Products.Common;
using CleanArchitectureMediatorShowcase.Domain.Common;
using Mediator;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.GetProducts;

/// <summary>
/// Query handler for searching and paginating products via read-optimized projection.
/// </summary>
public sealed class GetProductsQueryHandler(IProductReadRepository readRepository)
    : IQueryHandler<GetProductsQuery, Result<PagedList<ProductResponse>>>
{
    /// <inheritdoc />
    public async ValueTask<Result<PagedList<ProductResponse>>> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var criteria = new ProductSearchCriteria(
            query.SearchTerm,
            query.Page,
            query.PageSize,
            query.SortBy,
            query.SortDirection);

        var pagedList = await readRepository.SearchAsync(criteria, cancellationToken).ConfigureAwait(false);
        return pagedList;
    }
}
