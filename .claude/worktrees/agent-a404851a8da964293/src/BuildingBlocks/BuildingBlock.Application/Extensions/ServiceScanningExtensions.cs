using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Application.Extensions;

public static class ServiceScanningExtensions
{
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
