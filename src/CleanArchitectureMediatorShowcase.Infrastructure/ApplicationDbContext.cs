using CleanArchitectureMediatorShowcase.Domain.Abstractions;
using CleanArchitectureMediatorShowcase.Domain.Entities;
using CleanArchitectureMediatorShowcase.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureMediatorShowcase.Infrastructure;

/// <summary>
/// EF Core database context and Unit of Work implementation for the application.
/// Manages persistence transactions and entity mapping.
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    /// <summary>
    /// Gets the DbSet for products.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        var isSqlite = Database.IsSqlite();
        modelBuilder.ApplyConfiguration(new ProductConfiguration(isSqlite));
    }
}
