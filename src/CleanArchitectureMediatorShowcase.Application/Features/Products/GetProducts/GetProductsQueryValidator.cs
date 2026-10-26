using FluentValidation;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.GetProducts;

/// <summary>
/// Validator for <see cref="GetProductsQuery"/>.
/// Enforces pagination bounds and whitelists sorting properties to prevent SQL injection or unexpected sorting behaviors.
/// </summary>
public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    private static readonly string[] AllowedSortProperties = ["name", "price", "stock", "createdatutc"];
    private static readonly string[] AllowedSortDirections = ["asc", "desc"];

    /// <summary>
    /// Initializes validation rules for product search and pagination.
    /// </summary>
    public GetProductsQueryValidator()
    {
        RuleFor(q => q.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1.");

        RuleFor(q => q.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(q => q.SortBy)
            .Must(sortBy => string.IsNullOrWhiteSpace(sortBy) || AllowedSortProperties.Contains(sortBy.Trim().ToLowerInvariant()))
            .WithMessage("SortBy must be one of: 'name', 'price', 'stock', 'createdAtUtc'.");

        RuleFor(q => q.SortDirection)
            .Must(sortDirection => string.IsNullOrWhiteSpace(sortDirection) || AllowedSortDirections.Contains(sortDirection.Trim().ToLowerInvariant()))
            .WithMessage("SortDirection must be either 'asc' or 'desc'.");
    }
}
