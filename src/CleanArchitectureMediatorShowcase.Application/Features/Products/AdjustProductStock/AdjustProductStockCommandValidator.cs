using FluentValidation;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.AdjustProductStock;

/// <summary>
/// Validator for <see cref="AdjustProductStockCommand"/>.
/// </summary>
public sealed class AdjustProductStockCommandValidator : AbstractValidator<AdjustProductStockCommand>
{
    /// <summary>
    /// Initializes validation rules for stock adjustment.
    /// </summary>
    public AdjustProductStockCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Product ID must be provided.");

        RuleFor(c => c.Delta)
            .NotEqual(0).WithMessage("Stock adjustment delta must be non-zero.");
    }
}
