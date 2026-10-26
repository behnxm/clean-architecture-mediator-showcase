using CleanArchitectureMediatorShowcase.Domain.Common;

namespace CleanArchitectureMediatorShowcase.Domain.Errors;

/// <summary>
/// Domain-specific errors for the Product aggregate root.
/// </summary>
public static class ProductErrors
{
    /// <summary>
    /// Indicates that a product with the specified unique identifier does not exist.
    /// </summary>
    public static Error NotFound(Guid id) =>
        Error.NotFound("Product.NotFound", $"Product with identifier '{id}' was not found.");

    /// <summary>
    /// Indicates that the product name was empty or whitespace.
    /// </summary>
    public static readonly Error EmptyName =
        Error.Validation("Product.EmptyName", "Product name cannot be empty.");

    /// <summary>
    /// Indicates that the product name exceeded the allowable maximum length.
    /// </summary>
    public static readonly Error NameTooLong =
        Error.Validation("Product.NameTooLong", "Product name cannot exceed 200 characters.");

    /// <summary>
    /// Indicates that the product description exceeded the allowable maximum length.
    /// </summary>
    public static readonly Error DescriptionTooLong =
        Error.Validation("Product.DescriptionTooLong", "Product description cannot exceed 2000 characters.");

    /// <summary>
    /// Indicates that the product price was negative.
    /// </summary>
    public static readonly Error InvalidPrice =
        Error.Validation("Product.InvalidPrice", "Product price cannot be negative.");

    /// <summary>
    /// Indicates that the product stock quantity was negative.
    /// </summary>
    public static readonly Error InvalidStock =
        Error.Validation("Product.InvalidStock", "Product stock cannot be negative.");

    /// <summary>
    /// Indicates that a stock decrement would reduce inventory below zero.
    /// </summary>
    public static readonly Error InsufficientStock =
        Error.Conflict("Product.InsufficientStock", "The requested stock adjustment exceeds available inventory.");

    /// <summary>
    /// Indicates that a product with the same name already exists in the catalog.
    /// </summary>
    public static readonly Error DuplicateName =
        Error.Conflict("Product.DuplicateName", "A product with the same name already exists.");
}
