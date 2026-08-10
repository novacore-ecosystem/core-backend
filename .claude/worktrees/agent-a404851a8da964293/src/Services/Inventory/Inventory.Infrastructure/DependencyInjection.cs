using NovaCore.BuildingBlock.Infrastructure.Audit;
using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using NovaCore.BuildingBlock.Infrastructure.Extensions;
using NovaCore.BuildingBlock.Infrastructure.Messaging;
using NovaCore.BuildingBlock.Messaging.Abstractions;
using NovaCore.BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NovaCore.Inventory.Infrastructure.BackgroundJobs;
using NovaCore.Inventory.Infrastructure.Messaging.Consumers;

namespace NovaCore.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAppLogger()
            .AddBackgroundJobs(configuration)
            // Inventory now has an Outbox (added for audit tracking - Inventory/Warehouse are
            // IAuditable), so it uses the shared dual cleanup-job pair like Order/User/Audit,
            // rather than the Inbox-only registration it used when it only consumed events.
            .AddInboxOutboxCleanupJobs(configuration)
            .AddHttpAuditMetadataProvider("Inventory");

        // Register application event dispatcher (MediatR - for internal events)
        services.AddApplicationEventDispatcher();

        // Consumers must be registered before AddKafkaMessaging - their Topics are
        // discovered eagerly to configure the KafkaFlow consumer pipeline.
        services.AddMessagingConsumers();
        services.AddKafkaMessaging(configuration, "inventory-service");
        services.AddInboxOutboxInfrastructure(configuration);

        return services;
    }

    private static IServiceCollection AddMessagingConsumers(
        this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventConsumer, VariantCreatedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, VariantDeletedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductDeletedIntegrationEventConsumer>();

        return services;
    }
}
