using FluentValidation;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.DeleteProduct;

/// <summary>
/// Validator for <see cref="DeleteProductCommand"/>.
/// </summary>
public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    /// <summary>
    /// Initializes validation rules for product deletion.
    /// </summary>
    public DeleteProductCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Product ID must be provided.");
    }
}
