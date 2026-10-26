using CleanArchitectureMediatorShowcase.Application;
using CleanArchitectureMediatorShowcase.Application.Common.Behaviors;
using CleanArchitectureMediatorShowcase.Infrastructure;
using CleanArchitectureMediatorShowcase.Infrastructure.Database;
using CleanArchitectureMediatorShowcase.WebAPI.Endpoints;
using CleanArchitectureMediatorShowcase.WebAPI.Infrastructure;
using Mediator;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Exception handling and ProblemDetails (RFC 9457)
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Application and Infrastructure layers
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

// Mediator source-generated registration
// Configured with Scoped lifetime to align with Scoped DbContext dependencies.
// Pipeline behaviors execute in registration order: Logging (outermost) -> Validation (innermost) -> Handler.
builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.PipelineBehaviors = [typeof(LoggingBehavior<,>), typeof(ValidationBehavior<,>)];
});

// Configurable CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:3000"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Built-in OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Standard middleware pipeline
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseCors();

// Interactive API documentation
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Product Catalog API - Clean Architecture Reference");
    });
}

// Health check endpoints
// /health: lightweight liveness probe
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false
});

// /health/ready: readiness probe validating database connectivity
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

// Resource endpoints
app.MapProductEndpoints();

// Automatic migrations & seeding in development or when explicitly enabled
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(dbContext, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

app.Run();

/// <summary>
/// Expose Program class for WebApplicationFactory integration testing.
/// </summary>
public partial class Program;
