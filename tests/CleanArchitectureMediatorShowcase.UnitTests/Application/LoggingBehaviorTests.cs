using CleanArchitectureMediatorShowcase.Application.Common.Behaviors;
using CleanArchitectureMediatorShowcase.Application.Common.Options;
using CleanArchitectureMediatorShowcase.Application.Features.Products.CreateProduct;
using CleanArchitectureMediatorShowcase.Domain.Common;
using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CleanArchitectureMediatorShowcase.UnitTests.Application;

public sealed class LoggingBehaviorTests
{
    private readonly ILogger<LoggingBehavior<CreateProductCommand, Result<Guid>>> _logger =
        Substitute.For<ILogger<LoggingBehavior<CreateProductCommand, Result<Guid>>>>();

    [Fact]
    public async Task Handle_WhenRequestIsSlow_LogsWarning()
    {
        // Arrange: set threshold to 0ms so any request triggers slow warning
        var options = Options.Create(new LoggingBehaviorOptions { SlowRequestThresholdMilliseconds = 0 });
        var behavior = new LoggingBehavior<CreateProductCommand, Result<Guid>>(_logger, options);
        var command = new CreateProductCommand("Name", "Desc", 10m, 5);

        MessageHandlerDelegate<CreateProductCommand, Result<Guid>> next = async (msg, ct) =>
        {
            await Task.Delay(15, ct);
            return Result<Guid>.Success(Guid.NewGuid());
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Slow request detected")),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WhenRequestIsFast_DoesNotLogWarning()
    {
        // Arrange: high threshold (10,000ms)
        var options = Options.Create(new LoggingBehaviorOptions { SlowRequestThresholdMilliseconds = 10_000 });
        var behavior = new LoggingBehavior<CreateProductCommand, Result<Guid>>(_logger, options);
        var command = new CreateProductCommand("Name", "Desc", 10m, 5);

        MessageHandlerDelegate<CreateProductCommand, Result<Guid>> next = (msg, ct) =>
            ValueTask.FromResult(Result<Guid>.Success(Guid.NewGuid()));

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _logger.DidNotReceive().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
