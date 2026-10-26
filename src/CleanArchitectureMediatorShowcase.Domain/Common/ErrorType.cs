namespace CleanArchitectureMediatorShowcase.Domain.Common;

/// <summary>
/// Categorizes the type of domain or application error to facilitate HTTP status mapping.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Unexpected or unhandled domain failure. Typically maps to HTTP 500 Internal Server Error.
    /// </summary>
    Failure = 0,

    /// <summary>
    /// Input or business rule validation error. Maps to HTTP 400 Bad Request.
    /// </summary>
    Validation = 1,

    /// <summary>
    /// Requested entity or resource was not found. Maps to HTTP 404 Not Found.
    /// </summary>
    NotFound = 2,

    /// <summary>
    /// Conflict with the current state of the resource (e.g., unique constraint violation). Maps to HTTP 409 Conflict.
    /// </summary>
    Conflict = 3
}
