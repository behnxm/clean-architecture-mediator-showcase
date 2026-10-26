using System.Collections.Concurrent;
using CleanArchitectureMediatorShowcase.Application.Features.Products.Notifications;
using CleanArchitectureMediatorShowcase.Infrastructure;
using CleanArchitectureMediatorShowcase.Infrastructure.Database;
using CleanArchitectureMediatorShowcase.Infrastructure.Interceptors;
using Mediator;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanArchitectureMediatorShowcase.IntegrationTests;

/// <summary>
/// Thread-safe tracker capturing notifications published during integration test runs.
/// </summary>
public sealed class TestNotificationTracker
{
    public ConcurrentBag<ProductCreatedNotification> PublishedNotifications { get; } = [];
}

/// <summary>
/// Test notification handler capturing <see cref="ProductCreatedNotification"/> events in-memory.
/// </summary>
public sealed class TestProductCreatedNotificationHandler(TestNotificationTracker tracker)
    : INotificationHandler<ProductCreatedNotification>
{
    public ValueTask Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
    {
        tracker.PublishedNotifications.Add(notification);
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Custom <see cref="WebApplicationFactory{TProgram}"/> providing isolated SQLite database storage
/// and notification tracking for integration test scenarios.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseFilePath = Path.Combine(Path.GetTempPath(), $"test_catalog_{Guid.NewGuid():N}.db");
    public TestNotificationTracker NotificationTracker { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Register isolated SQLite file database for this test class
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                var auditInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
                var domainEventsInterceptor = sp.GetRequiredService<DispatchDomainEventsInterceptor>();

                options.UseSqlite($"Data Source={_databaseFilePath}")
                       .AddInterceptors(auditInterceptor, domainEventsInterceptor);
            });

            // Register test notification tracker and subscriber
            services.AddSingleton(NotificationTracker);
            services.AddScoped<INotificationHandler<ProductCreatedNotification>, TestProductCreatedNotificationHandler>();

            // Build service provider to apply migrations and seed initial data
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

            db.Database.Migrate();
            DatabaseSeeder.SeedAsync(db, logger).GetAwaiter().GetResult();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && File.Exists(_databaseFilePath))
        {
            try
            {
                File.Delete(_databaseFilePath);
            }
            catch
            {
                // Best-effort cleanup of temporary test database
            }
        }
    }
}
