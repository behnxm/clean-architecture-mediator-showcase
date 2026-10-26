# ADR 0006: Separate Read-Side Repository for Query Optimization

## Status
Accepted

## Context
In standard DDD repository implementations, queries load full aggregate roots with change tracking enabled, only to immediately map them to response DTOs and discard them. This causes:
- Unnecessary entity tracking overhead in EF Core's change tracker.
- Over-fetching: retrieving unneeded columns, internal state, and child collections.
- Rigid query models constrained by aggregate boundary invariants.

## Decision
We implement **CQRS Query Segregation** with two distinct repository abstractions:
1. **Write Repository (`IProductRepository`)**:
   - Owned by Domain layer.
   - Operations: `GetByIdAsync` (tracked), `AddAsync`, `Remove`, `ExistsByNameAsync`.
   - Used exclusively by commands modifying domain state.
2. **Read Repository (`IProductReadRepository`)**:
   - Owned by Application layer.
   - Operations: `GetByIdAsync`, `SearchAsync(ProductSearchCriteria)` returning `PagedList<ProductResponse>`.
   - Uses `AsNoTracking()`.
   - Uses Mapperly expression projections (`ProjectToResponse()`) to project directly from SQL tables into immutable DTOs without ever instantiating domain entity objects.
   - Implements SQL-safe parameterized LIKE evaluation (`EF.Functions.Like`) and whitelisted property sorting.

## Consequences
### Positive
- Substantially lower memory allocation and CPU overhead for read-heavy query workloads.
- Clean architectural alignment: queries never mutate state and do not require aggregate invariant checks.
- Read models are free to reshape queries (projections, aggregations, joins) without polluting domain aggregates.

### Negative
- Requires maintaining two repository interfaces and implementations for the Product resource.
