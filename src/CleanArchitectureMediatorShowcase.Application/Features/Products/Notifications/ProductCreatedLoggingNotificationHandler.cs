using Mediator;
using Microsoft.Extensions.Logging;

namespace CleanArchitectureMediatorShowcase.Application.Features.Products.Notifications;

/// <summary>
/// First notification handler for <see cref="ProductCreatedNotification"/>.
/// Demonstrates the Observer pattern / decoupled side-effects by logging product creation.
/// </summary>
public sealed class ProductCreatedLoggingNotificationHandler(
    ILogger<ProductCreatedLoggingNotificationHandler> logger)
    : INotificationHandler<ProductCreatedNotification>
{
    /// <inheritdoc />
    public ValueTask Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        logger.LogInformation(
            "[Notification Handler 1: Logging] Product '{ProductName}' ({ProductId}) created with price {Price:C}",
            notification.Name,
            notification.ProductId,
            notification.Price);

        return ValueTask.CompletedTask;
    }
}
