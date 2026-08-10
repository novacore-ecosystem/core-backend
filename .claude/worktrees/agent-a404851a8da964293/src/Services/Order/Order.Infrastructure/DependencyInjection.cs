using NovaCore.BuildingBlock.Contract.Protos.Inventory;
using NovaCore.BuildingBlock.Grpc.Client;
using NovaCore.BuildingBlock.Infrastructure.Audit;
using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using NovaCore.BuildingBlock.Infrastructure.Extensions;
using NovaCore.BuildingBlock.Infrastructure.Messaging;
using NovaCore.BuildingBlock.Messaging.Abstractions;
using NovaCore.BuildingBlock.Messaging.Kafka.Extensions;
using NovaCore.BuildingBlock.Saga.Abstractions;
using NovaCore.BuildingBlock.Saga.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NovaCore.Order.Application.Abstractions.Services;
using NovaCore.Order.Application.Features.Orders.Sagas.CreateOrderSaga.Steps;
using NovaCore.Order.Infrastructure.BackgroundJobs;
using NovaCore.Order.Infrastructure.Caching;
using NovaCore.Order.Infrastructure.GrpcClients;
using NovaCore.Order.Infrastructure.Messaging.Consumers;

namespace NovaCore.Order.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAppLogger()
            .AddRedisCache(configuration)
            .AddIdempotency(configuration)
            .AddScoped<ICartService, CartService>()
            .AddBackgroundJobs(configuration)
            .AddInboxOutboxCleanupJobs(configuration)
            .AddHttpAuditMetadataProvider("Order");

        // Register application event dispatcher (MediatR - for internal events)
        services.AddApplicationEventDispatcher();

        // gRPC client + saga orchestrator registration: no ordering constraint relative to
        // AddKafkaMessaging - OrderCreatedSagaConsumer (registered below) only depends on
        // ISender/IAppLogger, so AddKafkaMessaging's DiscoverConsumerTopics (which eagerly
        // constructs every registered consumer to read its Topics) never touches these. The
        // actual saga steps/orchestrator are resolved lazily by MediatR inside
        // RunCreateOrderSagaHandler, only when a message is really processed - see
        // docs/reference/create-order-saga.md.
        services.AddGrpcClients(configuration);
        services.AddCreateOrderSaga();

        // Consumers must be registered before AddKafkaMessaging - their Topics are
        // discovered eagerly to configure the KafkaFlow consumer pipeline.
        services.AddMessagingConsumers();
        services.AddKafkaMessaging(configuration, "order-service");
        services.AddInboxOutboxInfrastructure(configuration);

        return services;
    }

    private static IServiceCollection AddMessagingConsumers(
        this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventConsumer, VariantCreatedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, VariantUpdatedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, VariantDeletedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductUpdatedIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, ProductDeletedIntegrationEventConsumer>();

        // Drives CreateOrderSaga - see docs/reference/create-order-saga.md.
        services.AddScoped<IIntegrationEventConsumer, OrderCreatedSagaConsumer>();

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
    /// Steps are Scoped (they depend on Scoped repos/gRPC clients); the orchestrator itself is
    /// Singleton to match NovaCore.BuildingBlock.Saga.Extensions.SagaExtensions' own convention - it never
    /// caches steps/definitions between calls, so this is safe (see OrderCreatedSagaConsumer,
    /// which resolves the Scoped steps itself and passes them in per message). ISagaStore is
    /// registered separately in NovaCore.Order.Persistence (EfSagaStore).
    /// </summary>
    private static IServiceCollection AddCreateOrderSaga(this IServiceCollection services)
    {
        services.AddScoped<DeductInventoryStep>();
        services.AddScoped<ConfirmOrderStep>();
        services.AddSingleton<ISagaOrchestrator, SagaOrchestrator>();

        return services;
    }
}
