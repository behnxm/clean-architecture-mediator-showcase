# ADR 0005: SQLite Default with EF Core Migrations and Decimal Sorting Adaptation

## Status
Accepted

## Context
A showcase and interview reference project must be instantly runnable with zero external infrastructure (no Docker or cloud dependencies required just to evaluate the code). SQLite provides an embedded, self-contained relational database that runs across Windows, macOS, and Linux without installation. However, SQLite has well-known limitations in EF Core, specifically regarding decimal column sorting.

## Decision
1. **SQLite Default**: Default connection string is `Data Source=productcatalog.db`. Can be overridden via environment variables or configuration (`ConnectionStrings:Database`).
2. **EF Core Migrations**: Migrations are scaffolded using the local `dotnet-ef` tool via an `IDesignTimeDbContextFactory<ApplicationDbContext>`. The initial migration `InitialCreate` is committed to source control.
3. **Automatic Startup Migration & Seeding**: In Development mode (or when `Database:ApplyMigrationsOnStartup` is true), the Web API runs `dbContext.Database.Migrate()` and seeds 5 realistic hardware items via `Product.Create` if empty.
4. **SQLite Decimal Sorting Adaptation**:
   - **Caveat**: SQLite lacks a native `DECIMAL` data type. By default, EF Core maps `decimal` to SQLite `TEXT`. When executing `ORDER BY Price`, SQLite sorts lexicographically (e.g., `"100.0"` comes before `"20.0"`), producing incorrect pagination and sorting results.
   - **Solution**: In `ProductConfiguration`, when `Database.IsSqlite()`, we register a conversion to `double` (`HasConversion<double>()`). This maps to SQLite's native `REAL` data type, enabling correct numeric SQL sorting while maintaining full decimal precision in C# domain memory.

## Consequences
### Positive
- Zero external friction: clone repository and run `dotnet run`.
- Correct SQL `ORDER BY Price ASC/DESC` behavior in SQLite verified by automated integration tests.
- Clear separation between SQLite development mode and production database options (PostgreSQL / SQL Server).

### Negative
- Converting `decimal` to `double` for SQLite is a provider-specific accommodation; documentation must explicitly explain how to transition to PostgreSQL or SQL Server.
