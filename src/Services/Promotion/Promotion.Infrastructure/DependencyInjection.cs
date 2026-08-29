using NovaCore.BuildingBlock.Infrastructure.Audit;
using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using NovaCore.BuildingBlock.Infrastructure.Extensions;
using NovaCore.BuildingBlock.Infrastructure.Messaging;
using NovaCore.BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NovaCore.Promotion.Infrastructure.Configurations;

namespace NovaCore.Promotion.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPromotionConfigurations(configuration)
            .AddAppLogger()
            .AddRedisCache(configuration)
            .AddIdempotency(configuration)
            .AddInboxOutboxCleanupJobs(configuration)
            .AddHttpAuditMetadataProvider("Promotion");

        // Register application event dispatcher (MediatR - for internal events)
        services.AddApplicationEventDispatcher();

        // No integration event consumers yet - Promotion Service has nothing to consume from
        // other services in this bootstrap phase, and no domain events to publish (no entities
        // exist yet). Consumers/producers are added alongside whichever Phase 6 workflow first
        // needs one - see docs/promotion-service/phases/phase-6-infrastructure-integration.md.
        services.AddKafkaMessaging(configuration, "promotion-service");
        services.AddInboxOutboxInfrastructure(configuration);

        return services;
    }
}
