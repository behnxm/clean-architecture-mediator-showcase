namespace CleanArchitectureMediatorShowcase.Domain.Common;

/// <summary>
/// Static abstract interface allowing generic pipeline behaviors (such as validation behaviors)
/// to create typed failure results without requiring reflection or dynamic instantiation,
/// maintaining full compatibility with Native AOT compilation.
/// </summary>
/// <typeparam name="TSelf">The concrete failure result type implementing this contract.</typeparam>
public interface IFailureResult<out TSelf> where TSelf : IFailureResult<TSelf>
{
    /// <summary>
    /// Creates a new failure result containing the specified error.
    /// </summary>
    /// <param name="error">The error payload.</param>
    /// <returns>A new failed result instance of type <typeparamref name="TSelf"/>.</returns>
    static abstract TSelf CreateFailure(Error error);
}
