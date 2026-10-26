using CleanArchitectureMediatorShowcase.Domain.Common;
using CleanArchitectureMediatorShowcase.Domain.Errors;
using CleanArchitectureMediatorShowcase.Domain.Events;

namespace CleanArchitectureMediatorShowcase.Domain.Entities;

/// <summary>
/// Product aggregate root representing an item in the product catalog.
/// Implements rich domain modeling: encapsulates invariants, exposes factory methods,
/// uses private setters, and emits domain events upon state changes.
/// </summary>
public sealed class Product : BaseEntity<Guid>, IAuditableEntity
{
    /// <summary>
    /// Maximum allowed length for a product name.
    /// </summary>
    public const int NameMaxLength = 200;

    /// <summary>
    /// Maximum allowed length for a product description.
    /// </summary>
    public const int DescriptionMaxLength = 2000;

    /// <summary>
    /// Private parameterless constructor required by EF Core for materialization.
    /// </summary>
    private Product()
    {
    }

    /// <summary>
    /// Private constructor enforcing invariant checking via static factory methods.
    /// </summary>
    private Product(Guid id, string name, string description, decimal price, int stock)
        : base(id)
    {
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
    }

    /// <summary>
    /// Gets the product name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the product description.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the product price in decimal currency.
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// Gets the available inventory quantity.
    /// </summary>
    public int Stock { get; private set; }

    /// <summary>
    /// Gets the timestamp when the entity was created in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the timestamp when the entity was last modified in UTC, or null if untouched.
    /// </summary>
    public DateTime? LastModifiedAtUtc { get; private set; }

    /// <summary>
    /// Factory method for creating a new <see cref="Product"/> aggregate root.
    /// Validates domain invariants and raises <see cref="ProductCreatedDomainEvent"/>.
    /// Generates identity using time-ordered UUIDv7 (<see cref="Guid.CreateVersion7()"/>).
    /// </summary>
    /// <param name="name">Product title or name.</param>
    /// <param name="description">Product marketing or detailed description.</param>
    /// <param name="price">Unit price.</param>
    /// <param name="stock">Initial inventory stock count.</param>
    /// <returns>A successful <see cref="Result{Product}"/> or a failure error.</returns>
    public static Result<Product> Create(string name, string description, decimal price, int stock)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        var trimmedDescription = description?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            return ProductErrors.EmptyName;
        }

        if (trimmedName.Length > NameMaxLength)
        {
            return ProductErrors.NameTooLong;
        }

        if (trimmedDescription.Length > DescriptionMaxLength)
        {
            return ProductErrors.DescriptionTooLong;
        }

        if (price < 0)
        {
            return ProductErrors.InvalidPrice;
        }

        if (stock < 0)
        {
            return ProductErrors.InvalidStock;
        }

        var product = new Product(Guid.CreateVersion7(), trimmedName, trimmedDescription, price, stock);
        product.AddDomainEvent(new ProductCreatedDomainEvent(product.Id, product.Name, product.Price));

        return product;
    }

    /// <summary>
    /// Updates the mutable product details (Name, Description, Price).
    /// </summary>
    /// <param name="name">Updated product name.</param>
    /// <param name="description">Updated product description.</param>
    /// <param name="price">Updated unit price.</param>
    /// <returns>A successful <see cref="Result"/> or validation error.</returns>
    public Result Update(string name, string description, decimal price)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        var trimmedDescription = description?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            return ProductErrors.EmptyName;
        }

        if (trimmedName.Length > NameMaxLength)
        {
            return ProductErrors.NameTooLong;
        }

        if (trimmedDescription.Length > DescriptionMaxLength)
        {
            return ProductErrors.DescriptionTooLong;
        }

        if (price < 0)
        {
            return ProductErrors.InvalidPrice;
        }

        Name = trimmedName;
        Description = trimmedDescription;
        Price = price;

        return Result.Success();
    }

    /// <summary>
    /// Adjusts the inventory stock level by adding or subtracting the specified delta.
    /// Invariant: stock can never drop below zero.
    /// </summary>
    /// <param name="delta">Amount to increase (positive) or decrease (negative).</param>
    /// <returns>A successful <see cref="Result"/> or conflict error if insufficient stock.</returns>
    public Result AdjustStock(int delta)
    {
        if (Stock + delta < 0)
        {
            return ProductErrors.InsufficientStock;
        }

        Stock += delta;
        return Result.Success();
    }
}
