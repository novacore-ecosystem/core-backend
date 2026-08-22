using NovaCore.BuildingBlock.Infrastructure.Audit;
using NovaCore.BuildingBlock.Infrastructure.Extensions;
using NovaCore.BuildingBlock.Infrastructure.Messaging;
using NovaCore.BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.Content.Infrastructure;

/// <summary>
/// Deliberately minimal for this phase - no gRPC clients, background jobs, or Kafka consumers
/// (unlike Order/Product's fuller AddInfrastructure), since ContentService doesn't consume any
/// integration events yet and has no external service calls in this phase's representative
/// slice. It still wires the Outbox-backed Kafka producer pipeline and the HTTP audit metadata
/// provider, since content lifecycle events and the automatic audit trail are both core to what
/// this service does even in its skeleton form.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAppLogger()
            .AddHttpAuditMetadataProvider("Content");

        services.AddApplicationEventDispatcher();

        services.AddKafkaMessaging(configuration, "content-service");
        services.AddInboxOutboxInfrastructure(configuration);

        return services;
    }
}
