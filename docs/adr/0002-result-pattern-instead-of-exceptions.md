# ADR 0002: Result Pattern Instead of Exceptions for Business Logic

## Status
Accepted

## Context
Traditional .NET architectures frequently rely on custom exceptions (`NotFoundException`, `ValidationException`, `ConflictException`) for control flow and business rule rejections. While simple, throwing exceptions carries significant drawbacks:
- Severe performance cost: allocating stack traces on every expected validation or business rule rejection.
- Hidden control flow: method signatures do not indicate what failures can occur, turning code into an unpredictable web of `try-catch` blocks.
- Non-idiomatic HTTP mapping: middleware must catch exceptions and translate them into status codes after stack unwinding.

## Decision
We adopt the **Result Pattern** with typed errors (`Result` and `Result<T>`):
1. **Expected Business Failures are Values**: Operations that can fail due to domain rules return `Result` or `Result<T>` containing an `Error` record (Code, Description, ErrorType: Failure, Validation, NotFound, Conflict).
2. **Static Abstract Interface `IFailureResult<TSelf>`**:
   ```csharp
   public interface IFailureResult<out TSelf> where TSelf : IFailureResult<TSelf>
   {
       static abstract TSelf CreateFailure(Error error);
   }
   ```
   This C# 11+ feature enables open generic pipeline behaviors (such as `ValidationBehavior<TMessage, TResponse>`) to create strongly typed failed results dynamically **without reflection**, preserving 100% Native AOT compatibility.
3. **No Exceptions for Validation or Domain Rules**: FluentValidation failures are aggregated into a `ValidationError` record and returned immediately, short-circuiting the mediator pipeline cleanly.
4. **Exceptions Reserved for Exceptional Situations**: Bugs, database network partitions, and host crashes are handled by ASP.NET Core's `IExceptionHandler` (`GlobalExceptionHandler`), returning safe RFC 9457 HTTP 500 responses with diagnostic `traceId` values.

## Consequences
### Positive
- **Predictable Method Signatures**: `ValueTask<Result<ProductResponse>>` explicitly declares that failure is an expected outcome.
- **Zero Stack Allocation Overhead**: Validations and domain invariants execute at raw memory speed.
- **Native AOT Compatible**: Static abstract factories eliminate reflection-based `Activator.CreateInstance` calls in pipeline behaviors.
- **Direct RFC 9457 Mapping**: Domain error types map deterministically to HTTP 400, 404, 409, and 500 status codes.

### Negative
- Callers must inspect `result.IsSuccess` / `result.IsFailure` rather than relying on automated exception bubbles.
