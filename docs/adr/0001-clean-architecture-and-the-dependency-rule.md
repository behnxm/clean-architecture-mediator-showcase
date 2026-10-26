# ADR 0001: Clean Architecture and the Dependency Inversion Rule

## Status
Accepted

## Context
In enterprise application development, coupling domain logic to web frameworks, databases, or third-party mediator libraries leads to brittle software, high regression risk, and vendor lock-in. We need an architectural foundation that maximizes testability, maintainability, and domain model purity while allowing infrastructure technologies to evolve independently.

## Decision
We adopt **Clean Architecture** (also known as Onion Architecture or Hexagonal Architecture / Ports and Adapters) structured into four concentric layers:

1. **Domain Layer (`CleanArchitectureMediatorShowcase.Domain`)**:
   - Contains pure C# domain entities, aggregate roots, value objects, domain events, domain error definitions, and result types.
   - **Zero external dependencies**: Zero NuGet packages (specifically, no reference to EF Core, FluentValidation, or `Mediator.Abstractions`).
   - Owns persistence abstractions (`IProductRepository`, `IUnitOfWork`) via the **Dependency Inversion Principle (DIP)**.

2. **Application Layer (`CleanArchitectureMediatorShowcase.Application`)**:
   - Contains use case implementations (commands, queries, validators, pipeline behaviors, notification handlers, and DTO projections).
   - Depends **only** on the Domain layer and compile-time message interfaces (`Mediator.Abstractions`).
   - Does not reference infrastructure concerns (no EF Core, no SQLite, no HTTP).

3. **Infrastructure Layer (`CleanArchitectureMediatorShowcase.Infrastructure`)**:
   - Implements abstractions defined in Domain and Application (`ApplicationDbContext`, repositories, EF interceptors, migrations).
   - Depends on Domain and Application, but never on WebAPI.

4. **Web API Layer (`CleanArchitectureMediatorShowcase.WebAPI`)**:
   - The composition root and presentation layer. Uses ASP.NET Core Minimal APIs.
   - The **only** project referencing `Mediator.SourceGenerator`.
   - Thin delegation: endpoints inject `ISender` (least privilege), perform zero business logic, and return RFC 9457 `ProblemDetails` on failure.

## Consequences
### Positive
- **Independent of UI and Database**: Can swap UI (Minimal API to gRPC) or Database (SQLite to PostgreSQL/SQL Server) without changing a single line of domain or application logic.
- **Ultra-High Testability**: Domain rules and application handlers can be unit-tested without mocks, databases, or web servers.
- **Enforced via Automated Architecture Tests**: NetArchTest / reflection unit tests continuously enforce layer boundaries in CI.

### Negative
- Additional project scaffolding and abstraction ceremony compared to a single-project monolith. For simple CRUD without future business growth, this is recognized as over-engineering (see README).
