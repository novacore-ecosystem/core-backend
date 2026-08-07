using NovaCore.BuildingBlock.Infrastructure.Audit;
using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using NovaCore.BuildingBlock.Infrastructure.Extensions;
using NovaCore.BuildingBlock.Infrastructure.Messaging;
using NovaCore.BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.Shipping.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAppLogger()
            .AddRedisCache(configuration)
            .AddIdempotency(configuration)
            .AddInboxOutboxCleanupJobs(configuration)
            .AddHttpAuditMetadataProvider("Shipping");

        // Register application event dispatcher (MediatR - for internal events)
        services.AddApplicationEventDispatcher();

        // No integration event consumers registered yet: in this foundation phase ShippingService
        // has nothing to react to (Order does not yet publish a "shipment requested" trigger, and
        // no carrier webhook pipeline exists). AddKafkaMessaging/AddInboxOutboxInfrastructure are
        // still wired so the Outbox relay and Inbox retry hosted services - and their tables -
        // are live from day one. Consumers are added alongside whichever workflow first needs one.
        services.AddKafkaMessaging(configuration, "shipping-service");
        services.AddInboxOutboxInfrastructure(configuration);

        // No IShippingProviderClient implementation is registered: the abstraction exists as a
        // documented seam, but no carrier has been integrated yet - see Providers/IShippingProviderClient.cs.

        return services;
    }
}
