namespace CleanArchitectureMediatorShowcase.Domain.Common;

/// <summary>
/// Encapsulates error information including machine-readable code, human-readable description, and error category.
/// </summary>
public record Error(string Code, string Description, ErrorType Type)
{
    /// <summary>
    /// Represents no error (used for successful results).
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    /// <summary>
    /// Represents a null value error.
    /// </summary>
    public static readonly Error NullValue = new("General.NullValue", "A required value was null.", ErrorType.Validation);

    /// <summary>
    /// Creates a generic failure error.
    /// </summary>
    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);

    /// <summary>
    /// Creates a validation error.
    /// </summary>
    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);

    /// <summary>
    /// Creates a not-found error.
    /// </summary>
    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);

    /// <summary>
    /// Creates a conflict error.
    /// </summary>
    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
}
