using CleanArchitectureMediatorShowcase.Application.Common.Events;
using CleanArchitectureMediatorShowcase.Domain.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CleanArchitectureMediatorShowcase.Infrastructure.Interceptors;

/// <summary>
/// EF Core interceptor that dispatches domain events AFTER database transaction commit.
/// Collects all domain events from tracked entities, clears them from the aggregate roots,
/// and delegates dispatching to the application layer's <see cref="IDomainEventDispatcher"/>.
/// Executing post-commit guarantees at-most-once delivery (suitable for in-memory dispatching);
/// for guaranteed at-least-once delivery, an Outbox pattern should be used.
/// </summary>
public sealed class DispatchDomainEventsInterceptor(IDomainEventDispatcher dispatcher)
    : SaveChangesInterceptor
{
    private readonly IDomainEventDispatcher _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

    /// <inheritdoc />
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await DispatchEventsAsync(eventData.Context, cancellationToken).ConfigureAwait(false);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }

    private async Task DispatchEventsAsync(Microsoft.EntityFrameworkCore.DbContext context, CancellationToken cancellationToken)
    {
        // Find all tracked entities inheriting BaseEntity<Guid> with queued domain events
        var entitiesWithEvents = context.ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity<Guid> baseEntity && baseEntity.DomainEvents.Count != 0)
            .Select(e => (BaseEntity<Guid>)e.Entity)
            .ToList();

        if (entitiesWithEvents.Count == 0)
        {
            return;
        }

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Clear events prior to dispatch to prevent duplicate dispatching
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        await _dispatcher.DispatchAsync(domainEvents, cancellationToken).ConfigureAwait(false);
    }
}
