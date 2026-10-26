using System.Reflection;
using System.Xml.Linq;
using CleanArchitectureMediatorShowcase.Domain.Common;
using CleanArchitectureMediatorShowcase.Domain.Entities;
using CleanArchitectureMediatorShowcase.Infrastructure;
using FluentValidation;
using Mediator;
using Shouldly;
using Xunit;

namespace CleanArchitectureMediatorShowcase.ArchitectureTests;

public sealed class CleanArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(Product).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(CleanArchitectureMediatorShowcase.Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(ApplicationDbContext).Assembly;
    private static readonly Assembly WebApiAssembly = typeof(Program).Assembly;

    [Fact]
    public void Domain_ShouldNotReference_AnyOtherProjects()
    {
        var referencedAssemblies = DomainAssembly.GetReferencedAssemblies().Select(a => a.Name!).ToList();

        referencedAssemblies.ShouldNotContain(a => a.StartsWith("CleanArchitectureMediatorShowcase.Application", StringComparison.Ordinal));
        referencedAssemblies.ShouldNotContain(a => a.StartsWith("CleanArchitectureMediatorShowcase.Infrastructure", StringComparison.Ordinal));
        referencedAssemblies.ShouldNotContain(a => a.StartsWith("CleanArchitectureMediatorShowcase.WebAPI", StringComparison.Ordinal));
    }

    [Fact]
    public void Domain_ShouldNotReference_MediatorAbstractions()
    {
        var referencedAssemblies = DomainAssembly.GetReferencedAssemblies().Select(a => a.Name!).ToList();

        referencedAssemblies.ShouldNotContain(a => a.Contains("Mediator", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Domain_ShouldNotReference_EntityFrameworkCore()
    {
        var referencedAssemblies = DomainAssembly.GetReferencedAssemblies().Select(a => a.Name!).ToList();

        referencedAssemblies.ShouldNotContain(a => a.Contains("EntityFrameworkCore", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Domain_ShouldNotReference_FluentValidation()
    {
        var referencedAssemblies = DomainAssembly.GetReferencedAssemblies().Select(a => a.Name!).ToList();

        referencedAssemblies.ShouldNotContain(a => a.Contains("FluentValidation", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Application_ShouldNotReference_Infrastructure_Or_WebAPI_Or_EntityFrameworkCore()
    {
        var referencedAssemblies = ApplicationAssembly.GetReferencedAssemblies().Select(a => a.Name!).ToList();

        referencedAssemblies.ShouldNotContain(a => a.StartsWith("CleanArchitectureMediatorShowcase.Infrastructure", StringComparison.Ordinal));
        referencedAssemblies.ShouldNotContain(a => a.StartsWith("CleanArchitectureMediatorShowcase.WebAPI", StringComparison.Ordinal));
        referencedAssemblies.ShouldNotContain(a => a.Contains("EntityFrameworkCore", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Infrastructure_ShouldNotReference_WebAPI()
    {
        var referencedAssemblies = InfrastructureAssembly.GetReferencedAssemblies().Select(a => a.Name!).ToList();

        referencedAssemblies.ShouldNotContain(a => a.StartsWith("CleanArchitectureMediatorShowcase.WebAPI", StringComparison.Ordinal));
    }

    [Fact]
    public void Only_WebAPI_Project_ShouldReference_MediatorSourceGenerator()
    {
        var solutionDir = Directory.GetCurrentDirectory();
        while (solutionDir is not null && !File.Exists(Path.Combine(solutionDir, "CleanArchitectureMediatorShowcase.slnx")))
        {
            var parent = Directory.GetParent(solutionDir);
            solutionDir = parent?.FullName;
        }

        solutionDir.ShouldNotBeNull();

        var nonWebApiProjects = new[]
        {
            Path.Combine(solutionDir, "src", "CleanArchitectureMediatorShowcase.Domain", "CleanArchitectureMediatorShowcase.Domain.csproj"),
            Path.Combine(solutionDir, "src", "CleanArchitectureMediatorShowcase.Application", "CleanArchitectureMediatorShowcase.Application.csproj"),
            Path.Combine(solutionDir, "src", "CleanArchitectureMediatorShowcase.Infrastructure", "CleanArchitectureMediatorShowcase.Infrastructure.csproj")
        };

        foreach (var projectPath in nonWebApiProjects)
        {
            File.Exists(projectPath).ShouldBeTrue($"Project file {projectPath} should exist.");
            var doc = XDocument.Load(projectPath);
            var packageRefs = doc.Descendants("PackageReference")
                .Select(e => e.Attribute("Include")?.Value)
                .Where(v => v is not null)
                .ToList();

            packageRefs.ShouldNotContain("Mediator.SourceGenerator");
        }

        var webApiCsproj = Path.Combine(solutionDir, "src", "CleanArchitectureMediatorShowcase.WebAPI", "CleanArchitectureMediatorShowcase.WebAPI.csproj");
        var webApiDoc = XDocument.Load(webApiCsproj);
        var webApiPackageRefs = webApiDoc.Descendants("PackageReference")
            .Select(e => e.Attribute("Include")?.Value)
            .ToList();

        webApiPackageRefs.ShouldContain("Mediator.SourceGenerator");
    }

    [Fact]
    public void Handlers_ShouldBeSealed_AndEndWith_Handler()
    {
        var handlerTypes = ApplicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract &&
                t.GetInterfaces().Any(i => i.IsGenericType &&
                    (i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                     i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                     i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>) ||
                     i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))))
            .ToList();

        handlerTypes.Count.ShouldBeGreaterThan(0);

        foreach (var handler in handlerTypes)
        {
            handler.IsSealed.ShouldBeTrue($"Handler '{handler.Name}' should be sealed for devirtualization and security.");
            var cleanName = handler.Name.Contains('`') ? handler.Name[..handler.Name.IndexOf('`')] : handler.Name;
            cleanName.EndsWith("Handler", StringComparison.Ordinal).ShouldBeTrue();
        }
    }

    [Fact]
    public void PipelineBehaviors_ShouldBeSealed_AndEndWith_Behavior()
    {
        var behaviorTypes = ApplicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract &&
                t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>)))
            .ToList();

        behaviorTypes.Count.ShouldBeGreaterThan(0);

        foreach (var behavior in behaviorTypes)
        {
            behavior.IsSealed.ShouldBeTrue($"Behavior '{behavior.Name}' should be sealed.");
            var cleanName = behavior.Name.Contains('`') ? behavior.Name[..behavior.Name.IndexOf('`')] : behavior.Name;
            cleanName.EndsWith("Behavior", StringComparison.Ordinal).ShouldBeTrue();
        }
    }

    [Fact]
    public void Validators_ShouldBeSealed_AndEndWith_Validator()
    {
        var validatorTypes = ApplicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.BaseType is not null && t.BaseType.IsGenericType &&
                        t.BaseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
            .ToList();

        validatorTypes.Count.ShouldBeGreaterThan(0);

        foreach (var validator in validatorTypes)
        {
            validator.IsSealed.ShouldBeTrue($"Validator '{validator.Name}' should be sealed.");
            var cleanName = validator.Name.Contains('`') ? validator.Name[..validator.Name.IndexOf('`')] : validator.Name;
            cleanName.EndsWith("Validator", StringComparison.Ordinal).ShouldBeTrue();
        }
    }

    [Fact]
    public void Entities_ShouldNotHave_PublicSetters()
    {
        var entityTypes = DomainAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && IsSubclassOfRawGeneric(typeof(BaseEntity<>), t))
            .ToList();

        entityTypes.Count.ShouldBeGreaterThan(0);

        foreach (var entity in entityTypes)
        {
            var publicProperties = entity.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in publicProperties)
            {
                var setter = property.GetSetMethod(nonPublic: false);
                setter.ShouldBeNull($"Property '{entity.Name}.{property.Name}' has a public setter. Entities must use private/init setters to preserve encapsulation.");
            }
        }
    }

    private static bool IsSubclassOfRawGeneric(Type generic, Type? toCheck)
    {
        while (toCheck is not null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur)
            {
                return true;
            }
            toCheck = toCheck.BaseType;
        }
        return false;
    }
}
