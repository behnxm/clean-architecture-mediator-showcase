using CleanArchitectureMediatorShowcase.Domain.Abstractions;
using CleanArchitectureMediatorShowcase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureMediatorShowcase.Infrastructure.Repositories;

/// <summary>
/// Write-side repository implementation for <see cref="Product"/> aggregate roots.
/// Operations are tracked by the <see cref="ApplicationDbContext"/> change tracker for atomic transactional updates.
/// </summary>
public sealed class ProductRepository(ApplicationDbContext context) : IProductRepository
{
    private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);
        await _context.Products.AddAsync(product, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Remove(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        _context.Products.Remove(product);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        var trimmedName = name.Trim();

        return await _context.Products
            .AnyAsync(
                p => p.Name == trimmedName && (!excludeId.HasValue || p.Id != excludeId.Value),
                cancellationToken)
            .ConfigureAwait(false);
    }
}
