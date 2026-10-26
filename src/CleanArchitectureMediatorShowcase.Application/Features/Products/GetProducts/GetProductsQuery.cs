using CleanArchitectureMediatorShowcase.Application.Common.Models;
using CleanArchitectureMediatorShowcase.Application.Features.Products.Common;
using CleanArchitectureMediatorShowcase.Domain.Common;
using Mediator;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.GetProducts;

/// <summary>
/// Query to search, sort, and paginate product catalog records.
/// </summary>
/// <param name="SearchTerm">Optional search term matching product name or description.</param>
/// <param name="Page">1-based page number (default 1).</param>
/// <param name="PageSize">Number of items per page (default 10, max 100).</param>
/// <param name="SortBy">Field to sort by: name, price, stock, or createdAtUtc.</param>
/// <param name="SortDirection">Sorting direction: asc or desc.</param>
public sealed record GetProductsQuery(
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 10,
    string? SortBy = null,
    string? SortDirection = null) : IQuery<Result<PagedList<ProductResponse>>>;
