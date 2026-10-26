using System.Diagnostics;
using CleanArchitectureMediatorShowcase.Application.Common.Options;
using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArchitectureMediatorShowcase.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior implementing cross-cutting execution logging and slow request performance monitoring.
/// Implements the Decorator / Pipeline pattern via <see cref="IPipelineBehavior{TMessage, TResponse}"/>.
/// </summary>
/// <typeparam name="TMessage">The incoming message type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class LoggingBehavior<TMessage, TResponse>(
    ILogger<LoggingBehavior<TMessage, TResponse>> logger,
    IOptions<LoggingBehaviorOptions> options)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    private readonly LoggingBehaviorOptions _options = options?.Value ?? new LoggingBehaviorOptions();

    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);
        var requestName = typeof(TMessage).Name;

        logger.LogDebug("Executing request {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();
        var response = await next(message, cancellationToken).ConfigureAwait(false);
        stopwatch.Stop();

        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        if (elapsedMilliseconds > _options.SlowRequestThresholdMilliseconds)
        {
            logger.LogWarning(
                "Slow request detected: {RequestName} took {ElapsedMilliseconds} ms (threshold: {Threshold} ms)",
                requestName,
                elapsedMilliseconds,
                _options.SlowRequestThresholdMilliseconds);
        }
        else
        {
            logger.LogDebug(
                "Completed request {RequestName} in {ElapsedMilliseconds} ms",
                requestName,
                elapsedMilliseconds);
        }

        return response;
    }
}
