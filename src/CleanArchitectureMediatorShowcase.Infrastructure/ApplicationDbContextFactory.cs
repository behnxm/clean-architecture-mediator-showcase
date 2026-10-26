using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CleanArchitectureMediatorShowcase.Infrastructure;

/// <summary>
/// Design-time DbContext factory enabling local 'dotnet-ef' tooling to discover and scaffold migrations
/// independently from the WebAPI host.
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    /// <inheritdoc />
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlite("Data Source=productcatalog.db");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
