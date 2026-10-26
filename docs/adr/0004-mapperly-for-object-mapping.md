# ADR 0004: Riok.Mapperly for Object Mapping and Queryable Projection

## Status
Accepted

## Context
Mapping domain entities to API response DTOs can be performed manually, through reflection-based mappers (e.g., AutoMapper), or via compile-time source-generated mappers. AutoMapper introduces reflection overhead, difficult debugging, licensing changes in recent years, and poor Native AOT support. Manual mapping requires repetitive boilerplate and maintenance overhead.

## Decision
We select **Riok.Mapperly** (version 4.3.1, Apache-2.0 license):
1. **Compile-Time Roslyn Source Generator**: Generates transparent, optimized C# mapping methods at compile time.
2. **Object and Queryable Projections**: Supports both in-memory mapping (`ToResponse(this Product product)`) and EF Core SQL expression tree projections (`ProjectToResponse(this IQueryable<Product> query)`).
3. **Domain Event Encapsulation Handling**: Domain entities expose `DomainEvents` as an internal state collection. We configure `[MapperIgnoreSource(nameof(Product.DomainEvents))]` on object mapping methods to prevent unintended mapping warnings (`RMG020`) while maintaining clean DTO projections.

```csharp
[Mapper]
public static partial class ProductMapper
{
    [MapperIgnoreSource(nameof(Product.DomainEvents))]
    public static partial ProductResponse ToResponse(this Product product);

    public static partial IQueryable<ProductResponse> ProjectToResponse(this IQueryable<Product> query);
}
```

## Consequences
### Positive
- Permissive Apache-2.0 license.
- Zero reflection at runtime, Native AOT compatible.
- EF Core translates `ProjectToResponse()` directly into SQL `SELECT` projections, retrieving only the required columns from the database.
- Generated code can be viewed and debugged with standard breakpoints.

### Negative
- Requires partial classes and methods.
