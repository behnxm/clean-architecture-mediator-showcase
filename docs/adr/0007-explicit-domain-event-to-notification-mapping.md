# ADR 0007: Explicit Domain-Event-to-Notification Mapping

## Status
Accepted

## Context
When entities raise domain events (e.g. `ProductCreatedDomainEvent`), the application layer must notify interested internal components. Two dispatch strategies exist:
1. **Direct Notification Inheritance**: Making `IDomainEvent` inherit `INotification` from `Mediator.Abstractions`. This violates the Clean Architecture Dependency Rule because the Domain layer would depend on a third-party mediator package.
2. **Reflection-Based Generic Dispatch**: Using reflection (`Activator.CreateInstance` or `MakeGenericType`) to wrap domain events at runtime. This introduces performance overhead and breaks Native AOT compilation.

## Decision
We decouple the Domain event model from the Application mediator model by introducing `IDomainEventDispatcher` and implementing it with an **explicit pattern-matching `switch`**:
1. **Domain Event**: `ProductCreatedDomainEvent : IDomainEvent` (in Domain, pure C#, zero dependencies).
2. **Application Notification**: `ProductCreatedNotification(Guid ProductId, string Name, decimal Price) : INotification` (in Application).
3. **Dispatcher**: `MediatorDomainEventDispatcher` explicitly translates each domain event to its corresponding application notification using a C# pattern-matching switch:
   ```csharp
   private static INotification? TranslateDomainEvent(IDomainEvent domainEvent) =>
       domainEvent switch
       {
           ProductCreatedDomainEvent e => new ProductCreatedNotification(e.ProductId, e.Name, e.Price),
           _ => null
       };
   ```
4. **Post-Commit Dispatching**: Dispatched via `DispatchDomainEventsInterceptor` immediately after `SaveChangesAsync` commits the database transaction (at-most-once delivery).

## Consequences
### Positive
- **100% Native AOT Compatible**: Zero reflection or dynamic type invocation.
- **Strict Clean Architecture Boundary**: Domain layer remains completely isolated from mediator abstractions.
- **Compile-Time Type Safety**: The compiler verifies notification contracts and handler signatures.

### Negative
- **Scaling Trade-Off**: For systems with dozens or hundreds of domain events, maintaining a manual `switch` statement requires adding an entry for each event. However, for a reference architecture and enterprise applications prioritizing reliability, safety, and AOT support, explicit code is significantly easier to audit, test, and debug than reflection magic.
