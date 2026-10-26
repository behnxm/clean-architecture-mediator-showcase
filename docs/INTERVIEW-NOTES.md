# Clean Architecture & Mediator: Technical Interview Reference Guide

This document prepares senior engineers and architects for technical discussions, system design interviews, and code reviews based on this codebase.

---

### Question 1: Why did you use `martinothamar/Mediator` instead of MediatR or a hand-written dispatcher?
**Answer:**
- **License & Legal Safety**: In MediatR 13+, the project transitioned to RPL-1.5 / commercial licensing, creating legal constraints for enterprise commercial software unless using the feature-restricted Community edition. `martinothamar/Mediator` is 100% MIT-licensed.
- **Compile-Time vs. Runtime Reflection**: Traditional MediatR scans assemblies at startup and constructs runtime delegates via reflection and expression compilation. `martinothamar/Mediator` uses a Roslyn incremental source generator running at build time, emitting direct C# dispatch code in [Mediator.g.cs](file:///d:/Projects/Code/ant/CleanArchitectureMediatorShowcase/src/CleanArchitectureMediatorShowcase.WebAPI/Generated/Mediator.SourceGenerator/Mediator.SourceGenerator.IncrementalMediatorGenerator/Mediator.g.cs).
- **Native AOT & ValueTask**: `martinothamar/Mediator` is built for Native AOT and returns `ValueTask`, avoiding heap allocations when handlers complete synchronously.
- **Comparison to the Hand-Written Dispatcher Repo**: In our sibling repository (`clean-architecture-cqrs-from-scratch`), we wrote custom `ICommandHandler<TCommand, TResult>` interfaces and an in-memory dispatcher. While hand-written dispatchers give 100% control and zero external dependencies, ready-made libraries provide standardized pipeline behaviors, streaming request support, and ecosystem familiarity. Using `martinothamar/Mediator` delivers the benefits of an established library while retaining compile-time safety and zero reflection overhead.

---

### Question 2: Why does the Domain layer have ZERO NuGet packages—not even `Mediator.Abstractions`?
**Answer:**
- In Clean Architecture, the **Domain layer is the innermost core** of the application. It represents pure business logic and enterprise rules.
- Coupling the domain layer to a mediator library (for example, making `IDomainEvent` inherit `Mediator.INotification`) causes the domain to be vulnerable to third-party framework breaking changes, licensing shifts, or deprecation.
- By keeping [Domain](file:///d:/Projects/Code/ant/CleanArchitectureMediatorShowcase/src/CleanArchitectureMediatorShowcase.Domain) as pure C# (zero packages), we ensure business rules outlive any framework. The Application layer acts as the adapter, translating pure domain events into application notifications via [MediatorDomainEventDispatcher.cs](file:///d:/Projects/Code/ant/CleanArchitectureMediatorShowcase/src/CleanArchitectureMediatorShowcase.Application/Common/Events/MediatorDomainEventDispatcher.cs).

---

### Question 3: How does the Result pattern replace exceptions, and how is Native AOT preserved without reflection?
**Answer:**
- **Predictability & Performance**: Throwing exceptions for validation errors or missing entities incurs stack trace allocation overhead. The Result pattern ([Result.cs](file:///d:/Projects/Code/ant/CleanArchitectureMediatorShowcase/src/CleanArchitectureMediatorShowcase.Domain/Common/Result.cs)) turns failures into explicit return values.
- **Reflection-Free AOT Dispatch**: Open generic pipeline behaviors (like [ValidationBehavior.cs](file:///d:/Projects/Code/ant/CleanArchitectureMediatorShowcase/src/CleanArchitectureMediatorShowcase.Application/Common/Behaviors/ValidationBehavior.cs)) need to instantiate a failed `Result<T>` when validation rules fail. Typically, developers resort to `Activator.CreateInstance` or `MakeGenericType`, which breaks Native AOT.
- We solved this using C# 11+ **static abstract interface members**:
  ```csharp
  public interface IFailureResult<out TSelf> where TSelf : IFailureResult<TSelf>
  {
      static abstract TSelf CreateFailure(Error error);
  }
  ```
  `ValidationBehavior` constrains `where TResponse : Result, IFailureResult<TResponse>`, allowing it to call `TResponse.CreateFailure(validationError)` with direct compile-time static dispatch and zero reflection.

---

### Question 4: Why are there separate Write and Read repositories in this solution?
**Answer:**
- This demonstrates **CQRS (Command Query Responsibility Segregation)** at the data access level:
  - **Write Repository ([IProductRepository.cs](file:///d:/Projects/Code/ant/CleanArchitectureMediatorShowcase/src/CleanArchitectureMediatorShowcase.Domain/Abstractions/IProductRepository.cs))**: Owned by Domain. Manages aggregate roots with change tracking enabled (`GetByIdAsync`, `AddAsync`, `Remove`). Ensures business invariants and concurrency are enforced when state changes.
  - **Read Repository ([IProductReadRepository.cs](file:///d:/Projects/Code/ant/CleanArchitectureMediatorShowcase/src/CleanArchitectureMediatorShowcase.Application/Features/Products/Common/IProductReadRepository.cs))**: Owned by Application. Bypasses domain aggregates completely. Executes `AsNoTracking()` queries directly projecting database rows into flat `ProductResponse` DTOs via Mapperly.
- This prevents unnecessary entity tracking overhead and enables server-side SQL pagination, filtering, and sorting without aggregate ceremony.

---

### Question 5: How does Riok.Mapperly achieve database-level SQL projection in EF Core?
**Answer:**
- [ProductMapper.cs](file:///d:/Projects/Code/ant/CleanArchitectureMediatorShowcase/src/CleanArchitectureMediatorShowcase.Application/Features/Products/Common/ProductMapper.cs) defines:
  ```csharp
  [Mapper]
  public static partial class ProductMapper
  {
      public static partial IQueryable<ProductResponse> ProjectToResponse(this IQueryable<Product> query);
  }
  ```
- Rather than compiling IL at runtime like AutoMapper, Mapperly generates an `Expression<Func<Product, ProductResponse>>` at compile time.
- When invoked in [ProductReadRepository.cs](file:///d:/Projects/Code/ant/CleanArchitectureMediatorShowcase/src/CleanArchitectureMediatorShowcase.Infrastructure/Repositories/ProductReadRepository.cs), EF Core's query provider inspects the expression tree and translates it directly into SQL `SELECT Id, Name, Description, Price, Stock...`, retrieving only the projected columns from the database.

---

### Question 6: Why does Product have private setters and a private parameterless constructor?
**Answer:**
- **Encapsulation & Aggregate Invariants**: In DDD and Clean Architecture, an aggregate root must always be in a valid state. Public setters allow external callers to mutate fields arbitrarily, bypassing validation rules. In [Product.cs](file:///d:/Projects/Code/ant/CleanArchitectureMediatorShowcase/src/CleanArchitectureMediatorShowcase.Domain/Entities/Product.cs), all mutation occurs through explicit domain methods (`Create`, `Update`, `AdjustStock`).
- **ORM Requirement**: EF Core requires a parameterless constructor to instantiate entities during database materialization. Making `private Product() { }` satisfies EF Core while preventing application code from constructing an uninitialized product.

---

### Question 7: How are domain events dispatched, and why is this done post-commit instead of inside the transaction?
**Answer:**
- Aggregate roots queue domain events in an internal list ([BaseEntity.cs](file:///d:/Projects/Code/ant/CleanArchitectureMediatorShowcase/src/CleanArchitectureMediatorShowcase.Domain/Common/BaseEntity.cs)).
- In [DispatchDomainEventsInterceptor.cs](file:///d:/Projects/Code/ant/CleanArchitectureMediatorShowcase/src/CleanArchitectureMediatorShowcase.Infrastructure/Interceptors/DispatchDomainEventsInterceptor.cs), we hook into `SavedChangesAsync` (after the database transaction has committed).
- **Why Post-Commit?**: In-memory domain event handlers frequently trigger side-effects (e.g. sending emails, publishing metrics, notifying external services). If events were dispatched before commit and the database transaction rolled back, external systems would have acted on phantom state.
- **Trade-off**: Post-commit in-memory dispatch provides **at-most-once** delivery: if the host process crashes between commit and handler execution, the event is lost. For mission-critical transactional consistency, an **Outbox Pattern** with a durable table and background publisher should be used.

---

### Question 8: Why does SQLite require a decimal-to-double conversion, and what happens without it?
**Answer:**
- SQLite does not have a native `DECIMAL` numeric type. By default, EF Core maps C# `decimal` to SQLite `TEXT`.
- When querying `ORDER BY Price`, SQLite sorts lexicographically (alphabetical string sorting), where `"100.0"` is treated as smaller than `"20.0"`. This corrupts pagination and sorting.
- In [ProductConfiguration.cs](file:///d:/Projects/Code/ant/CleanArchitectureMediatorShowcase/src/CleanArchitectureMediatorShowcase.Infrastructure/Configurations/ProductConfiguration.cs), when `Database.IsSqlite()`, we apply `builder.Property(p => p.Price).HasConversion<double>()`. This maps to SQLite's native `REAL` type, enabling correct numeric SQL sorting while preserving high-precision decimals in C# memory. This is verified by our automated integration test `PriceSorting_CorrectlyOrdersNumericallyInSqlite`.

---

### Question 9: How do pipeline behaviors work in `martinothamar/Mediator`, and how do you ensure execution order?
**Answer:**
- Pipeline behaviors implement `IPipelineBehavior<TMessage, TResponse>`:
  ```csharp
  ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken);
  ```
- They are registered in [Program.cs](file:///d:/Projects/Code/ant/CleanArchitectureMediatorShowcase/src/CleanArchitectureMediatorShowcase.WebAPI/Program.cs) via `options.PipelineBehaviors = [typeof(LoggingBehavior<,>), typeof(ValidationBehavior<,>)];`.
- The source generator inspects this list and creates a recursive wrapper delegate where the **first listed behavior is the outermost wrapper**.
- When a request arrives, execution flow is: `LoggingBehavior (enter)` -> `ValidationBehavior (enter)` -> `Handler` -> `ValidationBehavior (exit)` -> `LoggingBehavior (exit)`. This order is proven by [PipelineExecutionOrderTests.cs](file:///d:/Projects/Code/ant/CleanArchitectureMediatorShowcase/tests/CleanArchitectureMediatorShowcase.UnitTests/Application/PipelineExecutionOrderTests.cs).

---

### Question 10: How are RFC 9457 ProblemDetails mapped from domain errors?
**Answer:**
- In [ResultExtensions.cs](file:///d:/Projects/Code/ant/CleanArchitectureMediatorShowcase/src/CleanArchitectureMediatorShowcase.WebAPI/Extensions/ResultExtensions.cs), `ToHttpResult()` inspects the `ErrorType` enum:
  - `ErrorType.Validation` -> HTTP 400 Bad Request with an `errors` dictionary grouping individual property errors.
  - `ErrorType.NotFound` -> HTTP 404 Not Found.
  - `ErrorType.Conflict` -> HTTP 409 Conflict.
  - `ErrorType.Failure` -> HTTP 500 Internal Server Error.
- Every response includes standardized `code` (e.g. `"Product.DuplicateName"`) and `traceId` (from `Activity.Current?.Id ?? HttpContext.TraceIdentifier`) extensions.

---

### Question 11: Why do the Minimal API endpoints inject `ISender` instead of `IMediator`?
**Answer:**
- Demonstrates the **Interface Segregation Principle (ISP)** and the **Principle of Least Privilege**:
  - `ISender`: Exposes only `Send(request, cancellationToken)`.
  - `IPublisher`: Exposes only `Publish(notification, cancellationToken)`.
  - `IMediator`: Inherits both `ISender` and `IPublisher`.
- HTTP endpoints only dispatch commands or queries; they never publish notifications directly (notifications are emitted by domain event interceptors). Injecting `ISender` restricts the API surface to what the endpoint actually requires.

---

### Question 12: When is Clean Architecture overkill, and how would you simplify this for a startup MVP?
**Answer:**
- **When It Is Overkill**:
  - Simple CRUD applications without complex domain logic.
  - Prototypes, short-lived microservices, and internal tooling with low change frequency.
  - Teams unfamiliar with DDD/Clean Architecture where ceremony slows delivery without delivering business value.
- **How to Simplify for an MVP**:
  1. Consolidate into a single project using Feature Folders (Vertical Slice Architecture).
  2. Use Minimal APIs with EF Core DbContext injected directly into endpoint handlers.
  3. Eliminate Repository and Unit of Work abstractions (EF Core's `DbSet` and `DbContext` already serve as Repository and Unit of Work).
  4. Retain validation and Result patterns to keep code clean and testable without the architectural overhead of 7 projects.
