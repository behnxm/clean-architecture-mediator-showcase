using CleanArchitectureMediatorShowcase.Application.Common.Models;
using CleanArchitectureMediatorShowcase.Application.Features.Products.Common;
using CleanArchitectureMediatorShowcase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureMediatorShowcase.Infrastructure.Repositories;

/// <summary>
/// Read-side repository bypassing aggregate tracking for query performance.
/// Utilizes Mapperly compile-time expression tree projections (<see cref="ProductMapper.ProjectToResponse"/>)
/// and SQL-safe parameterized LIKE evaluation.
/// </summary>
public sealed class ProductReadRepository(ApplicationDbContext context) : IProductReadRepository
{
    private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc />
    public async Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .ProjectToResponse()
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<PagedList<ProductResponse>> SearchAsync(
        ProductSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<Product> query = _context.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            var pattern = $"%{criteria.SearchTerm.Trim()}%";
            query = query.Where(p => EF.Functions.Like(p.Name, pattern) || EF.Functions.Like(p.Description, pattern));
        }

        var isDescending = string.Equals(criteria.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var sortBy = criteria.SortBy?.Trim().ToLowerInvariant() ?? "name";

        query = (sortBy, isDescending) switch
        {
            ("price", false) => query.OrderBy(p => p.Price),
            ("price", true) => query.OrderByDescending(p => p.Price),
            ("stock", false) => query.OrderBy(p => p.Stock),
            ("stock", true) => query.OrderByDescending(p => p.Stock),
            ("createdatutc", false) => query.OrderBy(p => p.CreatedAtUtc),
            ("createdatutc", true) => query.OrderByDescending(p => p.CreatedAtUtc),
            ("name", true) => query.OrderByDescending(p => p.Name),
            _ => query.OrderBy(p => p.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var page = Math.Max(1, criteria.Page);
        var pageSize = Math.Clamp(criteria.PageSize, 1, 100);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectToResponse()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return PagedList<ProductResponse>.Create(items, page, pageSize, totalCount);
    }
}
