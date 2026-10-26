using System.ComponentModel.DataAnnotations;
using CleanArchitectureMediatorShowcase.Domain.Entities;

namespace CleanArchitectureMediatorShowcase.WebAPI.Contracts.Requests;

/// <summary>
/// HTTP request payload for modifying existing product details.
/// </summary>
public sealed record UpdateProductRequest
{
    /// <summary>
    /// Updated product name.
    /// </summary>
    [Required]
    [MaxLength(Product.NameMaxLength)]
    public required string Name { get; init; }

    /// <summary>
    /// Updated product description.
    /// </summary>
    [MaxLength(Product.DescriptionMaxLength)]
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Updated unit sale price.
    /// </summary>
    [Range(0, 999_999_999)]
    public required decimal Price { get; init; }
}
