using CleanArchitectureMediatorShowcase.Domain.Common;

namespace CleanArchitectureMediatorShowcase.Domain.Events;

/// <summary>
/// Domain event published when a new product aggregate is created.
/// Encapsulates pure domain state without framework dependencies.
/// </summary>
/// <param name="ProductId">The unique identifier of the created product.</param>
/// <param name="Name">The name of the created product.</param>
/// <param name="Price">The unit price of the created product.</param>
public sealed record ProductCreatedDomainEvent(Guid ProductId, string Name, decimal Price) : IDomainEvent;
