using NovaCore.BuildingBlock.Infrastructure.Audit;
using NovaCore.BuildingBlock.Infrastructure.Extensions;
using NovaCore.BuildingBlock.Infrastructure.Messaging;
using NovaCore.BuildingBlock.Messaging.Kafka.Extensions;

using NovaCore.Content.Infrastructure.BackgroundJobs;
using NovaCore.Content.Infrastructure.Configurations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.Content.Infrastructure;

/// <summary>
/// Still deliberately minimal - no gRPC clients or Kafka consumers (unlike Order/Product's fuller
/// AddInfrastructure), since ContentService doesn't consume any integration events yet and has no
/// external service calls in this phase's representative slice. It wires the Outbox-backed Kafka
/// producer pipeline, the HTTP audit metadata provider, and the hard-delete retention background
/// job (the first background job this service needs).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddContentConfigurations(configuration)
            .AddAppLogger()
            .AddHttpAuditMetadataProvider("Content");

        services.AddApplicationEventDispatcher();

        services.AddKafkaMessaging(configuration, "content-service");
        services.AddInboxOutboxInfrastructure(configuration);
        services.AddBackgroundJobs(configuration);

        return services;
    }
}
