using CleanArchitectureMediatorShowcase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanArchitectureMediatorShowcase.Infrastructure.Database;

/// <summary>
/// Seeds realistic demo catalog items into the database during development or startup.
/// All entities are created via <see cref="Product.Create"/> ensuring business invariants are satisfied.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Checks if the products table is empty, and if so, seeds 5 realistic hardware catalog items.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task SeedAsync(
        ApplicationDbContext context,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);

        if (await context.Products.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        logger.LogInformation("Database is empty. Seeding initial product catalog entries...");

        Product[] seedProducts =
        [
            Product.Create(
                "Ergonomic Mechanical Keyboard",
                "Low-profile mechanical keyboard with hot-swappable switches and RGB backlighting.",
                149.99m,
                25).Value,

            Product.Create(
                "Precision Wireless Mouse",
                "Ultra-lightweight wireless ergonomic mouse with 26K DPI optical sensor.",
                79.99m,
                40).Value,

            Product.Create(
                "UltraSharp 27-inch 4K Monitor",
                "IPS panel with 99% sRGB color gamut and USB-C 90W power delivery.",
                449.00m,
                15).Value,

            Product.Create(
                "Active Noise-Cancelling Headphones",
                "Over-ear wireless headphones with spatial audio and 40-hour battery life.",
                299.50m,
                30).Value,

            Product.Create(
                "Thunderbolt 4 Docking Station",
                "Dual 4K display output, 100W charging pass-through, and Gigabit Ethernet.",
                199.95m,
                20).Value
        ];

        await context.Products.AddRangeAsync(seedProducts, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Successfully seeded {Count} products into catalog.", seedProducts.Length);
    }
}
