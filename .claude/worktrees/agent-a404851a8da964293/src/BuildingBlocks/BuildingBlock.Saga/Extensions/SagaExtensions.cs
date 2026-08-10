using NovaCore.BuildingBlock.Saga.Abstractions;
using NovaCore.BuildingBlock.Saga.Core;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Saga.Extensions;

/// <summary>
/// Extension methods for registering saga services.
/// </summary>
public static class SagaExtensions
{
    /// <summary>
    /// Add saga orchestration with in-memory store.
    /// For development and testing only.
    /// </summary>
    public static IServiceCollection AddSagaOrchestration(this IServiceCollection services)
    {
        services.AddSingleton<ISagaStore, InMemorySagaStore>();
        services.AddSingleton<ISagaOrchestrator, SagaOrchestrator>();
        return services;
    }

    /// <summary>
    /// Add saga orchestration with custom saga store.
    /// Use for production with persistent storage.
    /// </summary>
    public static IServiceCollection AddSagaOrchestration<TStore>(this IServiceCollection services)
        where TStore : class, ISagaStore
    {
        services.AddSingleton<ISagaStore, TStore>();
        services.AddSingleton<ISagaOrchestrator, SagaOrchestrator>();
        return services;
    }

    /// <summary>
    /// Add saga orchestration with a factory-created saga store.
    /// </summary>
    public static IServiceCollection AddSagaOrchestration(
        this IServiceCollection services,
        Func<IServiceProvider, ISagaStore> storeFactory)
    {
        services.AddSingleton(storeFactory);
        services.AddSingleton<ISagaOrchestrator, SagaOrchestrator>();
        return services;
    }
}
