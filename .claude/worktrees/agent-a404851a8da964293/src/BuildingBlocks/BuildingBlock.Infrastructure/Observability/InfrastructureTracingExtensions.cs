using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry.Instrumentation.StackExchangeRedis;
using OpenTelemetry.Trace;

using StackExchange.Redis;

namespace NovaCore.BuildingBlock.Infrastructure.Observability;

public static class InfrastructureTracingExtensions
{
    /// <summary>
    /// Registers the manual ActivitySource used by background hosted services (Outbox relay,
    /// Inbox retry) and enables Redis instrumentation. Call from each service's
    /// AddOpenTelemetryObservability(... configureTracing) callback in Program.cs.
    /// </summary>
    public static TracerProviderBuilder AddInfrastructureTracing(this TracerProviderBuilder builder)
    {
        return builder
            .AddSource(InfrastructureActivitySource.Name)
            .AddRedisInstrumentation();
    }

    /// <summary>
    /// StackExchangeRedis instrumentation needs the live IConnectionMultiplexer instance, which
    /// isn't available yet when AddInfrastructureTracing runs at TracerProviderBuilder
    /// configuration time - so it's wired here, once the app (and its DI container) exists. A
    /// no-op if this service didn't call AddRedisCache.
    /// </summary>
    public static WebApplication UseRedisTracing(this WebApplication app)
    {
        var multiplexer = app.Services.GetService<IConnectionMultiplexer>();
        var instrumentation = app.Services.GetService<StackExchangeRedisInstrumentation>();

        if (multiplexer is not null && instrumentation is not null)
            instrumentation.AddConnection(multiplexer);

        return app;
    }
}
