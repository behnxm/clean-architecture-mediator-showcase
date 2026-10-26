using System.Diagnostics;
using CleanArchitectureMediatorShowcase.Domain.Common;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CleanArchitectureMediatorShowcase.WebAPI.Extensions;

/// <summary>
/// Extension methods for converting domain and application <see cref="Result"/> objects
/// into strongly typed RFC 9457 <see cref="ProblemHttpResult"/> responses.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Transforms a failed domain <see cref="Result"/> into an RFC 9457 ProblemDetails HTTP response.
    /// Maps validation errors to HTTP 400 with an errors dictionary, not-found to HTTP 404, conflict to HTTP 409,
    /// and failure to HTTP 500, preserving error codes and diagnostic trace IDs.
    /// </summary>
    /// <param name="result">The failed result.</param>
    /// <param name="httpContext">The active HTTP context for trace ID propagation.</param>
    /// <returns>A strongly-typed <see cref="ProblemHttpResult"/> response.</returns>
    public static ProblemHttpResult ToHttpResult(this Result result, HttpContext? httpContext = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot convert a successful result into an error ProblemDetails response.");
        }

        return ToHttpResult(result.Error, httpContext);
    }

    /// <summary>
    /// Transforms an <see cref="Error"/> into an RFC 9457 ProblemDetails HTTP response.
    /// </summary>
    /// <param name="error">The error descriptor.</param>
    /// <param name="httpContext">The active HTTP context.</param>
    /// <returns>A strongly-typed <see cref="ProblemHttpResult"/> response.</returns>
    public static ProblemHttpResult ToHttpResult(this Error error, HttpContext? httpContext = null)
    {
        ArgumentNullException.ThrowIfNull(error);

        var traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;

        var extensions = new Dictionary<string, object?>
        {
            ["code"] = error.Code,
            ["traceId"] = traceId
        };

        if (error is ValidationError validationError)
        {
            var errorsDictionary = validationError.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(e => e.Description).ToArray());

            extensions["errors"] = errorsDictionary;
        }

        return error.Type switch
        {
            ErrorType.Validation => TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                detail: error.Description,
                extensions: extensions),

            ErrorType.NotFound => TypedResults.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: error.Description,
                extensions: extensions),

            ErrorType.Conflict => TypedResults.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: error.Description,
                extensions: extensions),

            _ => TypedResults.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Server Error",
                detail: error.Description,
                extensions: extensions)
        };
    }
}
