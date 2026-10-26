using CleanArchitectureMediatorShowcase.Application.Features.Products.CreateProduct;
using CleanArchitectureMediatorShowcase.Domain.Abstractions;
using CleanArchitectureMediatorShowcase.Domain.Entities;
using CleanArchitectureMediatorShowcase.Domain.Errors;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CleanArchitectureMediatorShowcase.UnitTests.Application;

public sealed class CreateProductCommandHandlerTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _handler = new CreateProductCommandHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ReturnsConflictError()
    {
        // Arrange
        var command = new CreateProductCommand("Existing Keyboard", "Description", 100m, 5);
        _repository.ExistsByNameAsync(command.Name, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.DuplicateName);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenValid_AddsProductAndSavesChanges()
    {
        // Arrange
        var command = new CreateProductCommand("New Keyboard", "Description", 100m, 5);
        _repository.ExistsByNameAsync(command.Name, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);

        await _repository.Received(1).AddAsync(
            Arg.Is<Product>(p => p.Name == "New Keyboard" && p.Price == 100m),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
