using CleanArchitectureMediatorShowcase.Domain.Common;
using FluentValidation;
using Mediator;

namespace CleanArchitectureMediatorShowcase.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior providing declarative input validation via FluentValidation before handler execution.
/// Follows the Fail-Fast and Result patterns: aggregates all validation errors into a <see cref="ValidationError"/>
/// and short-circuits the request pipeline without throwing costly exceptions.
/// Uses <see cref="IFailureResult{TSelf}"/> static abstract dispatch for Native AOT reflection-free execution.
/// </summary>
/// <typeparam name="TMessage">The incoming message type.</typeparam>
/// <typeparam name="TResponse">The result type constrained to <see cref="Result"/> and <see cref="IFailureResult{TSelf}"/>.</typeparam>
public sealed class ValidationBehavior<TMessage, TResponse>(IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
    where TResponse : Result, IFailureResult<TResponse>
{
    private readonly IEnumerable<IValidator<TMessage>> _validators = validators ?? [];

    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        if (!_validators.Any())
        {
            return await next(message, cancellationToken).ConfigureAwait(false);
        }

        var context = new ValidationContext<TMessage>(message);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))).ConfigureAwait(false);

        var errors = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .Select(failure => Error.Validation(failure.PropertyName, failure.ErrorMessage))
            .ToList();

        if (errors.Count != 0)
        {
            var validationError = new ValidationError(errors);
            return TResponse.CreateFailure(validationError);
        }

        return await next(message, cancellationToken).ConfigureAwait(false);
    }
}
