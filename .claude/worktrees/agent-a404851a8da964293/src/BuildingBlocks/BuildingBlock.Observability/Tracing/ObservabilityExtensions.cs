using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace NovaCore.BuildingBlock.Observability.Tracing;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddOpenTelemetryObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        Action<TracerProviderBuilder>? configureTracing = null)
    {
        var apmServerUrl = configuration["Observability:ApmServerUrl"] ?? "http://apm-server:8200";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .SetSampler(new AlwaysOnSampler())
                    .AddAspNetCoreInstrumentation(options => options.RecordException = true)
                    .AddHttpClientInstrumentation(options => options.RecordException = true)
                    // Composes with AddHttpClientInstrumentation: this package suppresses the
                    // inner HTTP span it would otherwise duplicate, so both can be registered
                    // together safely - see grpc-dotnet/opentelemetry-dotnet-contrib docs.
                    .AddGrpcClientInstrumentation()
                    .AddOtlpExporter(otlp => otlp.Endpoint = new Uri(apmServerUrl));

                configureTracing?.Invoke(tracing);
            });

        return services;
    }
}
