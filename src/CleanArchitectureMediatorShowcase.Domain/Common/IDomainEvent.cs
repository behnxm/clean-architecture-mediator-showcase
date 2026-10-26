namespace CleanArchitectureMediatorShowcase.Domain.Common;

/// <summary>
/// Marker interface representing a domain event raised by an aggregate root within the domain layer.
/// In Clean Architecture, domain events capture state changes as explicit domain concepts,
/// decoupled from any external messaging or mediator abstractions.
/// </summary>
public interface IDomainEvent
{
}
