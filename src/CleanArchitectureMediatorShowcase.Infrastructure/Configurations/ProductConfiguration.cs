using CleanArchitectureMediatorShowcase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitectureMediatorShowcase.Infrastructure.Configurations;

/// <summary>
/// EF Core entity type configuration for <see cref="Product"/>.
/// Encapsulates table mappings, column constraints, indexes, and SQLite numeric sorting adaptations.
/// </summary>
public sealed class ProductConfiguration(bool isSqlite = true) : IEntityTypeConfiguration<Product>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Name)
            .HasMaxLength(Product.NameMaxLength)
            .IsRequired();

        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.Property(p => p.Description)
            .HasMaxLength(Product.DescriptionMaxLength)
            .IsRequired();

        if (isSqlite)
        {
            // Caveat: SQLite lacks native decimal support and stores decimals as TEXT, causing alphabetical
            // sort orders (e.g., "100.0" sorts before "20.0"). Converting to double maps to a SQLite REAL
            // column, ensuring correct numeric ORDER BY evaluation while preserving high fidelity for catalog prices.
            builder.Property(p => p.Price)
                .HasConversion<double>()
                .IsRequired();
        }
        else
        {
            builder.Property(p => p.Price)
                .HasPrecision(18, 2)
                .IsRequired();
        }

        builder.Property(p => p.Stock)
            .IsRequired();

        builder.Property(p => p.CreatedAtUtc)
            .IsRequired();

        builder.Property(p => p.LastModifiedAtUtc);

        // Domain events are in-memory domain state; do not map to database columns
        builder.Ignore(p => p.DomainEvents);
    }
}
