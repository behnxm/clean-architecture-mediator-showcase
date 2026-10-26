using CleanArchitectureMediatorShowcase.Application.Common.Behaviors;
using CleanArchitectureMediatorShowcase.Application.Common.Options;
using CleanArchitectureMediatorShowcase.Application.Features.Products.CreateProduct;
using CleanArchitectureMediatorShowcase.Domain.Common;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CleanArchitectureMediatorShowcase.UnitTests.Application;

public sealed class PipelineExecutionOrderTests
{
    [Fact]
    public async Task Pipeline_ExecutesInOrder_OuterLoggingFirstThenInnerValidationThenHandler()
    {
        // Arrange
        var executionLog = new List<string>();

        // Outer behavior: LoggingBehavior
        var logger = Substitute.For<ILogger<LoggingBehavior<CreateProductCommand, Result<Guid>>>>();
        var loggingOptions = Options.Create(new LoggingBehaviorOptions { SlowRequestThresholdMilliseconds = 500 });
        var loggingBehavior = new LoggingBehavior<CreateProductCommand, Result<Guid>>(logger, loggingOptions);

        // Inner behavior: ValidationBehavior
        var validator = new CreateProductCommandValidator();
        var validationBehavior = new ValidationBehavior<CreateProductCommand, Result<Guid>>([validator]);

        // Command handler delegate (innermost target)
        var expectedId = Guid.NewGuid();
        MessageHandlerDelegate<CreateProductCommand, Result<Guid>> targetHandler = (msg, ct) =>
        {
            executionLog.Add("Handler");
            return ValueTask.FromResult(Result<Guid>.Success(expectedId));
        };

        // Middle delegate: ValidationBehavior wrapping the Handler
        MessageHandlerDelegate<CreateProductCommand, Result<Guid>> validationDelegate = async (msg, ct) =>
        {
            executionLog.Add("Validation_Enter");
            var res = await validationBehavior.Handle(msg, targetHandler, ct);
            executionLog.Add("Validation_Exit");
            return res;
        };

        // Outer call: LoggingBehavior wrapping the Validation delegate
        var command = new CreateProductCommand("Valid Name", "Valid Desc", 49.99m, 10);

        // Act
        executionLog.Add("Logging_Enter");
        var result = await loggingBehavior.Handle(command, validationDelegate, CancellationToken.None);
        executionLog.Add("Logging_Exit");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedId);

        // Execution order confirms: Logging (outermost) -> Validation -> Handler -> Validation -> Logging (outermost)
        executionLog.ShouldBe([
            "Logging_Enter",
            "Validation_Enter",
            "Handler",
            "Validation_Exit",
            "Logging_Exit"
        ]);
    }
}
