using CleanArchitectureMediatorShowcase.Domain.Entities;
using CleanArchitectureMediatorShowcase.Infrastructure;
using CleanArchitectureMediatorShowcase.Infrastructure.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Shouldly;
using Xunit;

namespace CleanArchitectureMediatorShowcase.UnitTests.Infrastructure;

public sealed class AuditableEntityInterceptorTests
{
    [Fact]
    public async Task SavingChanges_WhenEntityAdded_SetsCreatedAtUtcAndNullModifiedAtUtc()
    {
        // Arrange
        var initialTime = new DateTimeOffset(2026, 4, 1, 10, 0, 0, TimeSpan.Zero);
        var fakeTime = new FakeTimeProvider(initialTime);
        var interceptor = new AuditableEntityInterceptor(fakeTime);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("DataSource=:memory:")
            .AddInterceptors(interceptor)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        var product = Product.Create("Product 1", "Desc", 10m, 5).Value;

        // Act
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        // Assert
        product.CreatedAtUtc.ShouldBe(initialTime.UtcDateTime);
        product.LastModifiedAtUtc.ShouldBeNull();
    }

    [Fact]
    public async Task SavingChanges_WhenEntityModified_UpdatesLastModifiedAtUtc()
    {
        // Arrange
        var initialTime = new DateTimeOffset(2026, 4, 1, 10, 0, 0, TimeSpan.Zero);
        var fakeTime = new FakeTimeProvider(initialTime);
        var interceptor = new AuditableEntityInterceptor(fakeTime);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("DataSource=:memory:")
            .AddInterceptors(interceptor)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        var product = Product.Create("Product 1", "Desc", 10m, 5).Value;
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        // Act: Advance time by 3 hours and update product
        fakeTime.Advance(TimeSpan.FromHours(3));
        product.Update("Updated Product", "Updated Desc", 15m);
        await context.SaveChangesAsync();

        // Assert
        product.CreatedAtUtc.ShouldBe(initialTime.UtcDateTime);
        product.LastModifiedAtUtc.ShouldBe(initialTime.AddHours(3).UtcDateTime);
    }
}
