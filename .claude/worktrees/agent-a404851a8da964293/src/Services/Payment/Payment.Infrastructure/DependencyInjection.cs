using NovaCore.BuildingBlock.Infrastructure.Audit;
using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using NovaCore.BuildingBlock.Infrastructure.Extensions;
using NovaCore.BuildingBlock.Infrastructure.Messaging;
using NovaCore.BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAppLogger()
            .AddRedisCache(configuration)
            .AddIdempotency(configuration)
            .AddInboxOutboxCleanupJobs(configuration)
            .AddHttpAuditMetadataProvider("Payment");

        // Register application event dispatcher (MediatR - for internal events)
        services.AddApplicationEventDispatcher();

        // No integration event consumers yet - PaymentService has nothing to consume from other
        // services in this foundation phase (it only ever gets called into via
        // ReferenceType/ReferenceId, never subscribes to another module's events). Consumers are
        // added alongside whichever later-phase workflow first needs one - see
        // docs/services/payment-service.md, Planned phases.
        services.AddKafkaMessaging(configuration, "payment-service");
        services.AddInboxOutboxInfrastructure(configuration);

        return services;
    }
}
