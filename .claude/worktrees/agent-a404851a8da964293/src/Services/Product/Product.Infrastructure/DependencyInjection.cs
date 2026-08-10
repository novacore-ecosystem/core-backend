using NovaCore.BuildingBlock.Contract.Protos.Inventory;
using NovaCore.BuildingBlock.Grpc.Client;
using NovaCore.BuildingBlock.Infrastructure.Audit;
using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using NovaCore.BuildingBlock.Infrastructure.Extensions;
using NovaCore.BuildingBlock.Infrastructure.Messaging;

using NovaCore.BuildingBlock.Messaging.Abstractions;
using NovaCore.BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NovaCore.Product.Application.Abstractions.Services;
using NovaCore.Product.Infrastructure.BackgroundJobs;
using NovaCore.Product.Infrastructure.GrpcClients;
using NovaCore.Product.Infrastructure.Messaging.Consumers;

namespace NovaCore.Product.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAppLogger()
            .AddRedisCache(configuration)
            .AddIdempotency(configuration)
            .AddBackgroundJobs(configuration)
            .AddInboxOutboxCleanupJobs(configuration)
            .AddHttpAuditMetadataProvider("Product");

        services.AddApplicationEventDispatcher();
        services.AddGrpcClients(configuration);

        // Consumers must be registered before AddKafkaMessaging - their Topics are discovered
        // eagerly to configure the KafkaFlow consumer pipeline. AddInboxOutboxInfrastructure
        // must come after AddKafkaMessaging so its real Inbox delegate registration is last
        // and wins over AddKafkaMessaging's placeholder.
        services.AddMessagingConsumers();
        services.AddKafkaMessaging(configuration, "product-service");
        services.AddInboxOutboxInfrastructure(configuration);

        return services;
    }

    private static IServiceCollection AddGrpcClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var inventoryServiceUrl = configuration["Grpc:InventoryService:Url"] ?? "http://inventory-api:5002";

        services.AddGrpcClient<InventoryGrpcService.InventoryGrpcServiceClient>(new Uri(inventoryServiceUrl));
        services.AddScoped<IInventoryClientService, InventoryClientService>();

        return services;
    }

    /// <summary>
    /// Product now both publishes and self-consumes (Search index sync, see
    /// docs/reference/search.md) - one consumer per Product integration event, each a thin
    /// adapter dispatching an internal Search-sync/removal event, no business logic here.
    /// </summary>
    private static IServiceCollection AddMessagingConsumers(this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventConsumer, ProductCreatedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductUpdatedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductDeletedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, VariantCreatedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, VariantUpdatedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, VariantDeletedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductCategoryAssignedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductCategoryRemovedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductTagAssignedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductTagRemovedIntegrationEventConsumer>();
        return services;
    }
}
