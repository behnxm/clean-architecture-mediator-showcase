using System.ComponentModel.DataAnnotations;

namespace CleanArchitectureMediatorShowcase.WebAPI.Contracts.Requests;

/// <summary>
/// HTTP request payload for adjusting inventory stock levels.
/// </summary>
public sealed record AdjustStockRequest
{
    /// <summary>
    /// Quantity to add (positive) or deduct (negative) from current inventory.
    /// </summary>
    [Required]
    public required int Delta { get; init; }
}
