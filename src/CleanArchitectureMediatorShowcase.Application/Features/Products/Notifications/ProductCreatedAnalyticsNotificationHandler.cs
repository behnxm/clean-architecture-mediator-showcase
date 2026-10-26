using Mediator;
using Microsoft.Extensions.Logging;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.Notifications;

/// <summary>
/// Second notification handler for <see cref="ProductCreatedNotification"/>.
/// Demonstrates multiple independent subscribers for the same notification without modifying existing handlers
/// (Open-Closed Principle).
/// </summary>
public sealed class ProductCreatedAnalyticsNotificationHandler(
    ILogger<ProductCreatedAnalyticsNotificationHandler> logger)
    : INotificationHandler<ProductCreatedNotification>
{
    /// <inheritdoc />
    public ValueTask Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        logger.LogInformation(
            "[Notification Handler 2: Analytics] Recorded metric: New catalog entry {ProductId}",
            notification.ProductId);

        return ValueTask.CompletedTask;
    }
}
