using CleanArchitectureMediatorShowcase.Application.Features.Products.Notifications;
using CleanArchitectureMediatorShowcase.Domain.Common;
using CleanArchitectureMediatorShowcase.Domain.Events;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CleanArchitectureMediatorShowcase.Application.Common.Events;

/// <summary>
/// Dispatches domain events by explicitly translating each domain event type to its corresponding
/// application notification using pattern matching, then publishing through Mediator's <see cref="IPublisher"/>.
/// Avoiding reflection preserves Native AOT compatibility and provides compile-time safety.
/// </summary>
public sealed class MediatorDomainEventDispatcher(
    IPublisher publisher,
    ILogger<MediatorDomainEventDispatcher> logger)
    : IDomainEventDispatcher
{
    /// <inheritdoc />
    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        foreach (var domainEvent in domainEvents)
        {
            var notification = TranslateDomainEvent(domainEvent);
            if (notification is not null)
            {
                logger.LogDebug(
                    "Publishing notification {NotificationType} for domain event {DomainEventType}",
                    notification.GetType().Name,
                    domainEvent.GetType().Name);

                await publisher.Publish(notification, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                logger.LogWarning(
                    "No notification mapping defined for domain event {DomainEventType}",
                    domainEvent.GetType().Name);
            }
        }
    }

    private static INotification? TranslateDomainEvent(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            ProductCreatedDomainEvent e => new ProductCreatedNotification(e.ProductId, e.Name, e.Price),
            _ => null
        };
    }
}
