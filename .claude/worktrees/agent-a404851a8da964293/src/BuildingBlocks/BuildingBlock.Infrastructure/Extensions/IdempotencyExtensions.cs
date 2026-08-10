using NovaCore.BuildingBlock.Application.Abstractions.Idempotency;
using NovaCore.BuildingBlock.Infrastructure.Idempotency;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

namespace NovaCore.BuildingBlock.Infrastructure.Extensions;

/// <summary>
/// DI/pipeline registration for the shared Idempotency + Distributed Lock framework. Requires
/// Redis already registered (via <c>AddRedisCache</c>, or any other registration of
/// <see cref="IConnectionMultiplexer"/>/<c>ICacheService</c>) - this only wires the framework
/// on top of it, it never opens its own connection.
/// </summary>
public static class IdempotencyExtensions
{
    /// <summary>Add Idempotency + Distributed Lock from appsettings.json "Idempotency" section</summary>
    public static IServiceCollection AddIdempotency(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new IdempotencyOptions();
        configuration.GetSection(IdempotencyOptions.Section).Bind(options);
        return services.AddIdempotency(options);
    }

    /// <summary>Add Idempotency + Distributed Lock with explicit options</summary>
    public static IServiceCollection AddIdempotency(this IServiceCollection services, IdempotencyOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        if (!services.Any(d => d.ServiceType == typeof(IConnectionMultiplexer)))
            throw new InvalidOperationException(
                "AddIdempotency requires Redis already registered - call AddRedisCache(configuration) first.");

        services.AddSingleton(options);
        services.AddSingleton<IDistributedLockProvider, RedisDistributedLockProvider>();
        services.AddSingleton<IIdempotencyStore, RedisIdempotencyStore>();

        return services;
    }

    /// <summary>Enables the Idempotency middleware. Endpoints only enforce it when marked with <c>.RequireIdempotency()</c>.</summary>
    public static WebApplication UseIdempotency(this WebApplication app)
    {
        app.UseMiddleware<IdempotencyMiddleware>();
        return app;
    }
}
