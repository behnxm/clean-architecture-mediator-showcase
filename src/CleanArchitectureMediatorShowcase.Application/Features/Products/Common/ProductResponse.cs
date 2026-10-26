namespace CleanArchitectureMediatorShowcase.Application.Features.Products.Common;

/// <summary>
/// Data transfer object representing product catalog details.
/// Sealed immutable record keeping data flat for API consumption.
/// </summary>
/// <param name="Id">The unique product identifier.</param>
/// <param name="Name">Product title.</param>
/// <param name="Description">Product description.</param>
/// <param name="Price">Unit price.</param>
/// <param name="Stock">Available inventory stock.</param>
/// <param name="CreatedAtUtc">Creation timestamp in UTC.</param>
/// <param name="LastModifiedAtUtc">Last modified timestamp in UTC, or null.</param>
public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    DateTime CreatedAtUtc,
    DateTime? LastModifiedAtUtc);
