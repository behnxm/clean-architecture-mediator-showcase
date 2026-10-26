using CleanArchitectureMediatorShowcase.Domain.Entities;
using FluentValidation;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.CreateProduct;

/// <summary>
/// FluentValidation rules for <see cref="CreateProductCommand"/>.
/// Enforces business constraints before reaching the domain handler.
/// </summary>
public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    /// <summary>
    /// Initializes validation rules for product creation.
    /// </summary>
    public CreateProductCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Product name cannot be empty.")
            .MaximumLength(Product.NameMaxLength).WithMessage($"Product name cannot exceed {Product.NameMaxLength} characters.");

        RuleFor(c => c.Description)
            .NotNull().WithMessage("Product description cannot be null.")
            .MaximumLength(Product.DescriptionMaxLength).WithMessage($"Product description cannot exceed {Product.DescriptionMaxLength} characters.");

        RuleFor(c => c.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Product price cannot be negative.");

        RuleFor(c => c.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Product stock cannot be negative.");
    }
}
