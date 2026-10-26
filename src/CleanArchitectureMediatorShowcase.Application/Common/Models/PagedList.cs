namespace CleanArchitectureMediatorShowcase.Application.Common.Models;

/// <summary>
/// Generic paginated list container returned by query operations.
/// </summary>
/// <typeparam name="T">The type of item in the list.</typeparam>
/// <param name="Items">The items belonging to the current page.</param>
/// <param name="Page">The current 1-based page number.</param>
/// <param name="PageSize">The page size.</param>
/// <param name="TotalCount">The total count of items across all pages.</param>
public sealed record PagedList<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Gets a value indicating whether a subsequent page exists.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Gets a value indicating whether an anterior page exists.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Creates a new <see cref="PagedList{T}"/> instance.
    /// </summary>
    public static PagedList<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalCount) =>
        new(items ?? [], page, pageSize, totalCount);
}
