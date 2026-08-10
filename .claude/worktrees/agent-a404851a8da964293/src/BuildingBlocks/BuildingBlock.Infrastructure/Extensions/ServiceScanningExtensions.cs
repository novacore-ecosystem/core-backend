using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace NovaCore.BuildingBlock.Infrastructure.Extensions;

/// <summary>
/// Extension methods for automatic dependency injection service registration by scanning assemblies.
/// Wraps Scrutor library for simplified service scanning and registration.
/// Provides simple API: just pass service collection and assemblies to scan.
/// </summary>
public static class ServiceScanningExtensions
{
    /// <summary>
    /// Automatically discovers and registers all implementations of TInterface as scoped services.
    /// Uses Scrutor to scan assemblies and register all implementations with their interfaces.
    /// </summary>
    /// <typeparam name="TInterface">The interface type to scan for implementations.</typeparam>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="assembliesToScan">Types from assemblies to scan (uses their assembly for scanning).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddScopedByInterface<TInterface>(
        this IServiceCollection services,
        params Type[] assembliesToScan)
        where TInterface : class
    {
        services.Scan(scan => scan
            .FromAssembliesOf(assembliesToScan)
            .AddClasses(classes => classes.AssignableTo<TInterface>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    /// <summary>
    /// Automatically discovers and registers all implementations of TInterface as singleton services.
    /// Uses Scrutor to scan assemblies and register all implementations with their interfaces.
    /// </summary>
    /// <typeparam name="TInterface">The interface type to scan for implementations.</typeparam>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="assembliesToScan">Types from assemblies to scan (uses their assembly for scanning).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSingletonByInterface<TInterface>(
        this IServiceCollection services,
        params Type[] assembliesToScan)
        where TInterface : class
    {
        services.Scan(scan => scan
            .FromAssembliesOf(assembliesToScan)
            .AddClasses(classes => classes.AssignableTo<TInterface>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

        return services;
    }

    /// <summary>
    /// Automatically discovers and registers all implementations of a given interface type as scoped services.
    /// Non-generic overload for complex interface types (e.g., generic interfaces like IRepository&lt;&gt;).
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="interfaceType">The interface type to scan for implementations.</param>
    /// <param name="assembliesToScan">Types from assemblies to scan (uses their assembly for scanning).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddScopedByInterface(
        this IServiceCollection services,
        Type interfaceType,
        params Type[] assembliesToScan)
    {
        services.Scan(scan => scan
            .FromAssembliesOf(assembliesToScan)
            .AddClasses(classes => classes.AssignableTo(interfaceType))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    /// <summary>
    /// Automatically discovers and registers all implementations as both concrete and interface types (scoped).
    /// Useful when code needs to resolve by concrete type or interface type (e.g., Hangfire recurring jobs).
    /// </summary>
    /// <typeparam name="TInterface">The interface type to scan for implementations.</typeparam>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="assembliesToScan">Types from assemblies to scan (uses their assembly for scanning).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddScopedByInterfaceAndConcrete<TInterface>(
        this IServiceCollection services,
        params Type[] assembliesToScan)
        where TInterface : class
    {
        var implementations = assembliesToScan
            .SelectMany(t => t.Assembly.GetTypes())
            .Where(t => typeof(TInterface).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Distinct();

        foreach (var implementation in implementations)
        {
            services.AddScoped(implementation);
            services.AddScoped(typeof(TInterface), implementation);
        }

        return services;
    }

    /// <summary>
    /// Automatically discovers and registers all implementations as both concrete and interface types (singleton).
    /// Useful when code needs to resolve by concrete type or interface type.
    /// </summary>
    /// <typeparam name="TInterface">The interface type to scan for implementations.</typeparam>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="assembliesToScan">Types from assemblies to scan (uses their assembly for scanning).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSingletonByInterfaceAndConcrete<TInterface>(
        this IServiceCollection services,
        params Type[] assembliesToScan)
        where TInterface : class
    {
        var implementations = assembliesToScan
            .SelectMany(t => t.Assembly.GetTypes())
            .Where(t => typeof(TInterface).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Distinct();

        foreach (var implementation in implementations)
        {
            services.AddSingleton(implementation);
            services.AddSingleton(typeof(TInterface), implementation);
        }

        return services;
    }

    /// <summary>
    /// Automatically discovers and registers all implementations of a given interface type as singleton services.
    /// Non-generic overload for complex interface types (e.g., generic interfaces like IRepository&lt;&gt;).
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="interfaceType">The interface type to scan for implementations.</param>
    /// <param name="assembliesToScan">Types from assemblies to scan (uses their assembly for scanning).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSingletonByInterface(
        this IServiceCollection services,
        Type interfaceType,
        params Type[] assembliesToScan)
    {
        services.Scan(scan => scan
            .FromAssembliesOf(assembliesToScan)
            .AddClasses(classes => classes.AssignableTo(interfaceType))
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

        return services;
    }
}
