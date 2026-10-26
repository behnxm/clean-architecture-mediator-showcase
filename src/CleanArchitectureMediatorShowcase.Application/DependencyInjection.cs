using CleanArchitectureMediatorShowcase.Application.Common.Events;
using CleanArchitectureMediatorShowcase.Application.Common.Options;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitectureMediatorShowcase.Application;

/// <summary>
/// Dependency injection extensions for registering Application layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Application layer components including FluentValidation validators,
    /// domain event dispatcher, and options configuration.
    /// Note: Mediator handler discovery is handled by the compile-time source generator in the Web API entry assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Optional application configuration for options binding.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

        if (configuration is not null)
        {
            services.Configure<LoggingBehaviorOptions>(configuration.GetSection(LoggingBehaviorOptions.SectionName));
        }
        else
        {
            services.Configure<LoggingBehaviorOptions>(_ => { });
        }

        return services;
    }
}
