using CleanArchitectureMediatorShowcase.Application.Features.Products.Common;
using CleanArchitectureMediatorShowcase.Application.Features.Products.GetProductById;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CleanArchitectureMediatorShowcase.UnitTests.Application;

public sealed class GetProductByIdQueryHandlerTests
{
    private readonly IProductReadRepository _readRepository = Substitute.For<IProductReadRepository>();
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _handler = new GetProductByIdQueryHandler(_readRepository);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var query = new GetProductByIdQuery(id);
        _readRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((ProductResponse?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Product.NotFound");
    }

    [Fact]
    public async Task Handle_WhenFound_ReturnsProductResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedResponse = new ProductResponse(id, "Test Item", "Test Desc", 45m, 10, DateTime.UtcNow, null);
        var query = new GetProductByIdQuery(id);

        _readRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedResponse);
    }
}
