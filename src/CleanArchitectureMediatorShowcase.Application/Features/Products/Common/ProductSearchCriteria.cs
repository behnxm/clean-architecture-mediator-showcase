namespace CleanArchitectureMediatorShowcase.Application.Features.Products.Common;

/// <summary>
/// Criteria parameter object used for filtering, sorting, and paginating products.
/// </summary>
/// <param name="SearchTerm">Optional substring to filter product names or descriptions.</param>
/// <param name="Page">1-based page index.</param>
/// <param name="PageSize">Number of items per page.</param>
/// <param name="SortBy">Property name to sort by (name, price, stock, createdAtUtc).</param>
/// <param name="SortDirection">Sorting direction ("asc" or "desc").</param>
public sealed record ProductSearchCriteria(
    string? SearchTerm,
    int Page,
    int PageSize,
    string? SortBy,
    string? SortDirection);
