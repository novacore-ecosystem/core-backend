using OpenTelemetry.Trace;

namespace NovaCore.BuildingBlock.Persistence.Mongo.DependencyInjection;

public static class MongoTracingExtensions
{
    /// <summary>
    /// The ActivitySource name emitted by MongoDB.Driver.Core.Extensions.DiagnosticSources'
    /// DiagnosticsActivityEventSubscriber, wired in ServiceCollectionExtensions.
    /// </summary>
    private const string MongoActivitySourceName = "MongoDB.Driver.Core.Extensions.DiagnosticSources";

    public static TracerProviderBuilder AddMongoTracing(this TracerProviderBuilder builder)
    {
        return builder.AddSource(MongoActivitySourceName);
    }
}
