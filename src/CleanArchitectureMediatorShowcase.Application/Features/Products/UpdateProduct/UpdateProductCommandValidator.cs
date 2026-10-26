using CleanArchitectureMediatorShowcase.Domain.Entities;
using FluentValidation;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.UpdateProduct;

/// <summary>
/// FluentValidation rules for <see cref="UpdateProductCommand"/>.
/// </summary>
public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    /// <summary>
    /// Initializes validation rules for updating a product.
    /// </summary>
    public UpdateProductCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Product ID must be provided.");

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Product name cannot be empty.")
            .MaximumLength(Product.NameMaxLength).WithMessage($"Product name cannot exceed {Product.NameMaxLength} characters.");

        RuleFor(c => c.Description)
            .NotNull().WithMessage("Product description cannot be null.")
            .MaximumLength(Product.DescriptionMaxLength).WithMessage($"Product description cannot exceed {Product.DescriptionMaxLength} characters.");

        RuleFor(c => c.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Product price cannot be negative.");
    }
}
