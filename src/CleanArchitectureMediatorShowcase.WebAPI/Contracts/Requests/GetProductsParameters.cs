namespace CleanArchitectureMediatorShowcase.WebAPI.Contracts.Requests;

/// <summary>
/// Query string parameters for searching, sorting, and paginating products.
/// Discovered via Minimal API [AsParameters] binding.
/// </summary>
/// <param name="SearchTerm">Optional search keyword matching product title or description.</param>
/// <param name="Page">1-based page index (defaults to 1).</param>
/// <param name="PageSize">Number of items to retrieve per page (defaults to 10, max 100).</param>
/// <param name="SortBy">Property name to sort by: 'name', 'price', 'stock', or 'createdAtUtc'.</param>
/// <param name="SortDirection">Sorting order: 'asc' or 'desc'.</param>
public sealed record GetProductsParameters(
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 10,
    string? SortBy = null,
    string? SortDirection = null);
