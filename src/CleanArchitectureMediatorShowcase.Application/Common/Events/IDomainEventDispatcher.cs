using CleanArchitectureMediatorShowcase.Domain.Common;

namespace CleanArchitectureMediatorShowcase.Application.Common.Events;

/// <summary>
/// Application abstraction responsible for dispatching domain events originating from domain aggregates
/// to application-level notification handlers.
/// Decouples the domain layer from mediator notification infrastructure.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches a batch of domain events to their corresponding application notification handlers.
    /// </summary>
    /// <param name="domainEvents">The collection of domain events to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
