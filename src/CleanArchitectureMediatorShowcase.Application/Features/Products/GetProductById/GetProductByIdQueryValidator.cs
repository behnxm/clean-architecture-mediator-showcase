using FluentValidation;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.GetProductById;

/// <summary>
/// Validator for <see cref="GetProductByIdQuery"/>.
/// </summary>
public sealed class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    /// <summary>
    /// Initializes validation rules for retrieving a product by identifier.
    /// </summary>
    public GetProductByIdQueryValidator()
    {
        RuleFor(q => q.Id)
            .NotEmpty().WithMessage("Product ID must be provided.");
    }
}
