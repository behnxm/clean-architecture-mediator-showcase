# ADR 0003: Choosing martinothamar/Mediator Over Alternatives

## Status
Accepted

## Context
When adopting the Mediator and CQRS patterns in Clean Architecture, teams frequently default to libraries without evaluating licensing, runtime overhead, or Native AOT readiness. In recent years:
1. `MediatR` 13+ changed licensing from Apache-2.0 to RPL-1.5 / commercial licensing, creating legal constraints for proprietary commercial software unless using the feature-restricted Community version.
2. Older MediatR versions (12.x) remain Apache-2.0 but are frozen and rely heavily on runtime reflection and runtime expression tree compilation, impeding Native AOT compilation.
3. Other libraries (LiteBus, Cortex.Mediator, Wolverine) offer varying trade-offs in licensing, complexity, and messaging semantics.

## Decision
We select **`martinothamar/Mediator`** (version 3.0.2, MIT license):
- **Compile-Time Roslyn Source Generator**: Scans referenced projects at build time and generates strongly typed dispatch wrappers (`Mediator.g.cs`). Zero runtime reflection, zero IL emit, zero delegate compilation.
- **Allocation-Free `ValueTask`**: Handler and pipeline methods return `ValueTask` / `ValueTask<T>`, avoiding heap allocations for synchronously completing handlers.
- **Native AOT Ready**: Completely compatible with .NET ahead-of-time compilation.
- **Least-Privilege Segregated Interfaces**: Endpoints inject `ISender` (send-only commands/queries), whereas event dispatchers inject `IPublisher` (notification publishing), honoring Interface Segregation Principle (ISP).
- **Clean Architecture Boundary Discipline**:
  - `Domain`: References ZERO mediator packages (pure C#).
  - `Application`: References only `Mediator.Abstractions` (interfaces: `ICommand`, `IQuery`, `ICommandHandler`, `IQueryHandler`, `INotification`, `INotificationHandler`, `IPipelineBehavior`).
  - `WebAPI`: The only project referencing `Mediator.SourceGenerator` (as `PrivateAssets="all"`).

### API Configuration Deviation Note
In older documentation for `martinothamar/Mediator` (prior to v3.x), the DI lifetime configuration property was named `DefaultServiceLifetime`. In version 3.x, the property was renamed to `ServiceLifetime`. In addition, the source generator emits code specifically tailored to the configured lifetime:
```csharp
services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.PipelineBehaviors = [typeof(LoggingBehavior<,>), typeof(ValidationBehavior<,>)];
});
```
We set `ServiceLifetime = ServiceLifetime.Scoped` because our handlers depend on the scoped `ApplicationDbContext` via repositories.

## Alternatives Evaluated

| Library | License | Dispatch Mechanism | Return Type | Native AOT | Clean Architecture Fit |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **martinothamar/Mediator** | **MIT** | **Compile-time Roslyn Generator** | `ValueTask` | **Native AOT Ready** | **Gold Standard** (clean abstraction separation) |
| **MediatR 14.x / 13.x** | RPL-1.5 / Commercial | Runtime reflection & cached delegates | `Task` | Partial / warnings | Commercial licensing risk |
| **MediatR 12.4.x** | Apache-2.0 | Runtime reflection & expressions | `Task` | Poor | Feature-frozen, legacy codebase |
| **LiteBus** | MIT | Runtime descriptor registry | `Task` / `ValueTask` | Partial | Good, but runtime-based |
| **Wolverine** | Apache-2.0 | Runtime Roslyn codegen & compilation | Dynamic | Not AOT friendly | Full messaging bus; heavyweight for in-process CQRS |
| **Cortex.Mediator** | MIT | Runtime service resolution | `Task` | Partial | Lacks maturity of compile-time codegen |

## Consequences
### Positive
- Fully permissive MIT license with zero commercial risk.
- High runtime performance and zero reflection overhead.
- Developers can inspect generated code in `WebAPI/Generated/Mediator.SourceGenerator/`.
- F12 navigation directly accesses handlers or generated code.

### Negative
- Source generator compilation overhead during rebuilds (minimal with incremental generator).
