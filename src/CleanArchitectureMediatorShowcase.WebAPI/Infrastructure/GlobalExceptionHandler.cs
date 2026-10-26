using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureMediatorShowcase.WebAPI.Infrastructure;

/// <summary>
/// Global exception handler intercepting unhandled exceptions and transforming them into
/// RFC 9457 ProblemDetails responses with diagnostic trace IDs.
/// Prevents internal stack traces from leaking to API consumers.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        _logger.LogError(exception, "An unhandled exception occurred during request execution: {Message}", exception.Message);

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred",
            Detail = "An unexpected internal server error occurred. Please refer to the trace ID when contacting support.",
            Instance = httpContext.Request.Path,
            Extensions =
            {
                ["code"] = "General.InternalError",
                ["traceId"] = traceId
            }
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken).ConfigureAwait(false);

        return true;
    }
}
