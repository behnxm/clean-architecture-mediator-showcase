namespace CleanArchitectureMediatorShowcase.Domain.Common;

/// <summary>
/// Specialized error representing one or more validation failures, typically aggregated across FluentValidation rules.
/// </summary>
public sealed record ValidationError : Error
{
    /// <summary>
    /// Initializes a new instance of <see cref="ValidationError"/> with the collection of individual errors.
    /// </summary>
    /// <param name="errors">The list of distinct validation errors encountered.</param>
    public ValidationError(IReadOnlyList<Error> errors)
        : base("General.Validation", "One or more validation errors occurred.", ErrorType.Validation)
    {
        Errors = errors ?? [];
    }

    /// <summary>
    /// Gets the detailed list of individual validation errors.
    /// </summary>
    public IReadOnlyList<Error> Errors { get; }
}
