using System.ComponentModel.DataAnnotations;
using CleanArchitectureMediatorShowcase.Domain.Entities;

namespace CleanArchitectureMediatorShowcase.WebAPI.Contracts.Requests;

/// <summary>
/// HTTP request payload for creating a new product catalog item.
/// </summary>
public sealed record CreateProductRequest
{
    /// <summary>
    /// Product display title.
    /// </summary>
    [Required]
    [MaxLength(Product.NameMaxLength)]
    public required string Name { get; init; }

    /// <summary>
    /// Marketing or technical product description.
    /// </summary>
    [MaxLength(Product.DescriptionMaxLength)]
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Unit sale price.
    /// </summary>
    [Range(0, 999_999_999)]
    public required decimal Price { get; init; }

    /// <summary>
    /// Initial inventory quantity in stock.
    /// </summary>
    [Range(0, int.MaxValue)]
    public required int Stock { get; init; }
}
