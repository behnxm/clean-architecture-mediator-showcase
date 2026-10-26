using System.Diagnostics.CodeAnalysis;

namespace CleanArchitectureMediatorShowcase.Domain.Common;

/// <summary>
/// Encapsulates the outcome of an operation without throwing exceptions for expected business failures.
/// Implements <see cref="IFailureResult{TSelf}"/> using C# static abstracts for reflection-free AOT pipeline behaviors.
/// </summary>
public class Result : IFailureResult<Result>
{
    /// <summary>
    /// Initializes a new instance of <see cref="Result"/>.
    /// </summary>
    /// <param name="isSuccess">True if operation succeeded; false otherwise.</param>
    /// <param name="error">The associated error if failed, or <see cref="Error.None"/> if successful.</param>
    /// <exception cref="InvalidOperationException">Thrown if success and error state conflict.</exception>
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("A successful result cannot contain an error.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException("A failed result must provide an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error associated with the failed operation, or <see cref="Error.None"/> if successful.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Creates a successful <see cref="Result"/>.
    /// </summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a failed <see cref="Result"/> with the given error.
    /// </summary>
    /// <param name="error">The error description.</param>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a successful <see cref="Result{TValue}"/> containing the specified value.
    /// </summary>
    /// <typeparam name="TValue">The result payload type.</typeparam>
    /// <param name="value">The value payload.</param>
    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);

    /// <summary>
    /// Creates a failed <see cref="Result{TValue}"/> with the given error.
    /// </summary>
    /// <typeparam name="TValue">The result payload type.</typeparam>
    /// <param name="error">The error description.</param>
    public static Result<TValue> Failure<TValue>(Error error) => Result<TValue>.Failure(error);

    /// <inheritdoc />
    public static Result CreateFailure(Error error) => Failure(error);

    /// <summary>
    /// Implicit conversion from an <see cref="Domain.Common.Error"/> to a failed <see cref="Result"/>.
    /// </summary>
    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Encapsulates the outcome of an operation that produces a value of type <typeparamref name="TValue"/>.
/// </summary>
/// <typeparam name="TValue">The type of the returned value.</typeparam>
public class Result<TValue> : Result, IFailureResult<Result<TValue>>
{
    private readonly TValue? _value;

    /// <summary>
    /// Initializes a new instance of <see cref="Result{TValue}"/>.
    /// </summary>
    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the payload value when successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing value of a failed result.</exception>
    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

    /// <summary>
    /// Creates a successful <see cref="Result{TValue}"/>.
    /// </summary>
    public static Result<TValue> Success(TValue value) => new(value, true, Error.None);

    /// <summary>
    /// Creates a failed <see cref="Result{TValue}"/>.
    /// </summary>
    public static new Result<TValue> Failure(Error error) => new(default, false, error);

    /// <inheritdoc />
    public static new Result<TValue> CreateFailure(Error error) => Failure(error);

    /// <summary>
    /// Implicit conversion from a value to a successful <see cref="Result{TValue}"/>.
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) =>
        value is not null ? Success(value) : Failure(Error.NullValue);

    /// <summary>
    /// Implicit conversion from an <see cref="Domain.Common.Error"/> to a failed <see cref="Result{TValue}"/>.
    /// </summary>
    public static implicit operator Result<TValue>(Error error) => Failure(error);
}
