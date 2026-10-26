using CleanArchitectureMediatorShowcase.Application.Common.Behaviors;
using CleanArchitectureMediatorShowcase.Application.Features.Products.CreateProduct;
using CleanArchitectureMediatorShowcase.Domain.Common;
using FluentValidation;
using Mediator;
using Shouldly;
using Xunit;

namespace CleanArchitectureMediatorShowcase.UnitTests.Application;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WhenValidationFails_ShortCircuitsAndNeverCallsNext()
    {
        // Arrange
        var command = new CreateProductCommand(string.Empty, "Desc", -10m, -5);
        var validator = new CreateProductCommandValidator();
        var behavior = new ValidationBehavior<CreateProductCommand, Result<Guid>>([validator]);

        var nextCalled = false;
        MessageHandlerDelegate<CreateProductCommand, Result<Guid>> next = (msg, ct) =>
        {
            nextCalled = true;
            return ValueTask.FromResult(Result<Guid>.Success(Guid.NewGuid()));
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextCalled.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        var validationError = result.Error.ShouldBeOfType<ValidationError>();
        validationError.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_WhenValidationPasses_InvokesNextAndReturnsResult()
    {
        // Arrange
        var command = new CreateProductCommand("Valid Name", "Desc", 100m, 10);
        var validator = new CreateProductCommandValidator();
        var behavior = new ValidationBehavior<CreateProductCommand, Result<Guid>>([validator]);

        var expectedId = Guid.NewGuid();
        var nextCalled = false;
        MessageHandlerDelegate<CreateProductCommand, Result<Guid>> next = (msg, ct) =>
        {
            nextCalled = true;
            return ValueTask.FromResult(Result<Guid>.Success(expectedId));
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextCalled.ShouldBeTrue();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedId);
    }
}
