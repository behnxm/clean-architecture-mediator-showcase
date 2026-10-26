using CleanArchitectureMediatorShowcase.Application.Features.Products.Common;
using CleanArchitectureMediatorShowcase.Domain.Abstractions;
using CleanArchitectureMediatorShowcase.Infrastructure.Interceptors;
using CleanArchitectureMediatorShowcase.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CleanArchitectureMediatorShowcase.Infrastructure;

/// <summary>
/// Dependency injection registrations for the Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers persistence components, EF Core DbContext with interceptors, repositories, and health checks.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Configuration settings containing connection strings.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Register system clock if not already provided (e.g. by test doubles)
        services.TryAddSingleton(TimeProvider.System);

        // Interceptors
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        // Connection string: defaults to SQLite "Data Source=productcatalog.db"
        var connectionString = configuration.GetConnectionString("Database") ?? "Data Source=productcatalog.db";

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
            var domainEventsInterceptor = sp.GetRequiredService<DispatchDomainEventsInterceptor>();

            options.UseSqlite(connectionString)
                   .AddInterceptors(auditInterceptor, domainEventsInterceptor);
        });

        // Unit of Work and Repositories
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();

        // EF Core Health Checks
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(
                name: "database",
                tags: ["ready", "db"]);

        return services;
    }
}
