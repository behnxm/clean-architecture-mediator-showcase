using Mediator;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.Notifications;

/// <summary>
/// Application notification triggered when a product is created.
/// Implements Mediator's <see cref="INotification"/> contract to allow multiple independent subscribers.
/// </summary>
/// <param name="ProductId">Unique identifier of the created product.</param>
/// <param name="Name">Name of the created product.</param>
/// <param name="Price">Price of the created product.</param>
public sealed record ProductCreatedNotification(Guid ProductId, string Name, decimal Price) : INotification;
