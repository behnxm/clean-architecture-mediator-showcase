using CleanArchitectureMediatorShowcase.Application.Features.Products.UpdateProduct;
using CleanArchitectureMediatorShowcase.Domain.Abstractions;
using CleanArchitectureMediatorShowcase.Domain.Entities;
using CleanArchitectureMediatorShowcase.Domain.Errors;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CleanArchitectureMediatorShowcase.UnitTests.Application;

public sealed class UpdateProductCommandHandlerTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _handler = new UpdateProductCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new UpdateProductCommand(id, "Updated Name", "Updated Desc", 120m);
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Product.NotFound");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNameCollidesWithAnotherProduct_ReturnsConflictError()
    {
        // Arrange
        var product = Product.Create("Original Name", "Desc", 100m, 5).Value;
        var command = new UpdateProductCommand(product.Id, "Conflicting Name", "Desc", 120m);

        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _repository.ExistsByNameAsync("Conflicting Name", product.Id, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.DuplicateName);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenValid_UpdatesProductAndSavesChanges()
    {
        // Arrange
        var product = Product.Create("Original Name", "Desc", 100m, 5).Value;
        var command = new UpdateProductCommand(product.Id, "Updated Name", "Updated Desc", 120m);

        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _repository.ExistsByNameAsync("Updated Name", product.Id, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Name.ShouldBe("Updated Name");
        product.Price.ShouldBe(120m);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
