using CleanArchitectureMediatorShowcase.Application.Features.Products.Common;
using CleanArchitectureMediatorShowcase.Domain.Entities;
using Shouldly;
using Xunit;

namespace CleanArchitectureMediatorShowcase.UnitTests.Application;

public sealed class ProductMapperTests
{
    [Fact]
    public void ToResponse_MapsProductPropertiesCorrectly()
    {
        // Arrange
        var product = Product.Create("Mechanical Keyboard", "RGB Gaming", 149.99m, 20).Value;

        // Act
        var response = product.ToResponse();

        // Assert
        response.ShouldNotBeNull();
        response.Id.ShouldBe(product.Id);
        response.Name.ShouldBe("Mechanical Keyboard");
        response.Description.ShouldBe("RGB Gaming");
        response.Price.ShouldBe(149.99m);
        response.Stock.ShouldBe(20);
        response.CreatedAtUtc.ShouldBe(product.CreatedAtUtc);
        response.LastModifiedAtUtc.ShouldBe(product.LastModifiedAtUtc);
    }
}
