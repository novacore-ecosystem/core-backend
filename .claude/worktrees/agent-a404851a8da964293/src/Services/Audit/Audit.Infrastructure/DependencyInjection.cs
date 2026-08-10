using NovaCore.Audit.Application.Abstractions.Services;
using NovaCore.Audit.Infrastructure.BackgroundJobs;
using NovaCore.Audit.Infrastructure.GrpcClients;
using NovaCore.Audit.Infrastructure.Messaging.Consumers;

using NovaCore.BuildingBlock.Contract.Protos.User;
using NovaCore.BuildingBlock.Grpc.Client;
using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using NovaCore.BuildingBlock.Infrastructure.Extensions;
using NovaCore.BuildingBlock.Infrastructure.Messaging;
using NovaCore.BuildingBlock.Messaging.Abstractions;
using NovaCore.BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.Audit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAppLogger()
            .AddBackgroundJobs(configuration)
            .AddInboxOutboxCleanupJobs(configuration);

        // Register application event dispatcher (MediatR - for internal events)
        services.AddApplicationEventDispatcher();

        // Consumers must be registered before AddKafkaMessaging - their Topics are
        // discovered eagerly to configure the KafkaFlow consumer pipeline.
        services.AddMessagingConsumers();
        services.AddKafkaMessaging(configuration, "audit-service");
        services.AddInboxOutboxInfrastructure(configuration);

        services.AddGrpcClients(configuration);

        return services;
    }

    private static IServiceCollection AddMessagingConsumers(
        this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventConsumer, AuditIntegrationEventConsumer>();

        return services;
    }

    // Audit's first-ever gRPC client - see docs/tasks/2026-07-28/Task15_first-grpc-consumer.md.
    private static IServiceCollection AddGrpcClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var userServiceUrl = configuration["Grpc:UserService:Url"] ?? "http://user-api:5002";

        services.AddGrpcClient<UserGrpcService.UserGrpcServiceClient>(new Uri(userServiceUrl));
        services.AddScoped<IUserClientService, UserClientService>();

        return services;
    }
}
