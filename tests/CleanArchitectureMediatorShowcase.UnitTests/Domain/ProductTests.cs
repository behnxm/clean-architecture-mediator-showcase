using CleanArchitectureMediatorShowcase.Domain.Entities;
using CleanArchitectureMediatorShowcase.Domain.Errors;
using CleanArchitectureMediatorShowcase.Domain.Events;
using Shouldly;
using Xunit;

namespace CleanArchitectureMediatorShowcase.UnitTests.Domain;

public sealed class ProductTests
{
    [Fact]
    public void Create_WithValidParameters_ReturnsSuccessAndEmitsEvent()
    {
        // Act
        var result = Product.Create("Mechanical Keyboard", "High-end keyboard", 149.99m, 10);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var product = result.Value;
        product.Id.ShouldNotBe(Guid.Empty);
        product.Name.ShouldBe("Mechanical Keyboard");
        product.Description.ShouldBe("High-end keyboard");
        product.Price.ShouldBe(149.99m);
        product.Stock.ShouldBe(10);

        product.DomainEvents.Count.ShouldBe(1);
        var domainEvent = product.DomainEvents[0].ShouldBeOfType<ProductCreatedDomainEvent>();
        domainEvent.ProductId.ShouldBe(product.Id);
        domainEvent.Name.ShouldBe(product.Name);
        domainEvent.Price.ShouldBe(product.Price);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyOrWhitespaceName_ReturnsEmptyNameError(string? invalidName)
    {
        // Act
        var result = Product.Create(invalidName!, "Description", 99m, 5);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.EmptyName);
    }

    [Fact]
    public void Create_WithNameExceedingMaxLength_ReturnsNameTooLongError()
    {
        // Arrange
        var longName = new string('A', Product.NameMaxLength + 1);

        // Act
        var result = Product.Create(longName, "Description", 99m, 5);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.NameTooLong);
    }

    [Fact]
    public void Create_WithNegativePrice_ReturnsInvalidPriceError()
    {
        // Act
        var result = Product.Create("Mouse", "Description", -1m, 5);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.InvalidPrice);
    }

    [Fact]
    public void Create_WithNegativeStock_ReturnsInvalidStockError()
    {
        // Act
        var result = Product.Create("Mouse", "Description", 10m, -5);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.InvalidStock);
    }

    [Fact]
    public void Update_WithValidParameters_UpdatesProduct()
    {
        // Arrange
        var product = Product.Create("Old Name", "Old Desc", 50m, 10).Value;

        // Act
        var result = product.Update("New Name", "New Desc", 75m);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Name.ShouldBe("New Name");
        product.Description.ShouldBe("New Desc");
        product.Price.ShouldBe(75m);
    }

    [Fact]
    public void AdjustStock_WithPositiveDelta_IncreasesInventory()
    {
        // Arrange
        var product = Product.Create("Monitor", "Description", 300m, 10).Value;

        // Act
        var result = product.AdjustStock(5);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Stock.ShouldBe(15);
    }

    [Fact]
    public void AdjustStock_WithNegativeDeltaWithinBounds_DecreasesInventory()
    {
        // Arrange
        var product = Product.Create("Monitor", "Description", 300m, 10).Value;

        // Act
        var result = product.AdjustStock(-4);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Stock.ShouldBe(6);
    }

    [Fact]
    public void AdjustStock_WithExcessiveNegativeDelta_ReturnsInsufficientStockError()
    {
        // Arrange
        var product = Product.Create("Monitor", "Description", 300m, 10).Value;

        // Act
        var result = product.AdjustStock(-15);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.InsufficientStock);
        product.Stock.ShouldBe(10); // Unchanged
    }

    [Fact]
    public void ClearDomainEvents_RemovesQueuedEvents()
    {
        // Arrange
        var product = Product.Create("Trackpad", "Description", 100m, 5).Value;
        product.DomainEvents.Count.ShouldBe(1);

        // Act
        product.ClearDomainEvents();

        // Assert
        product.DomainEvents.Count.ShouldBe(0);
    }
}
