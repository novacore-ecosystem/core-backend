using NovaCore.BuildingBlock.Contract.Protos.Auth;
using NovaCore.BuildingBlock.Grpc.Client;
using NovaCore.BuildingBlock.Infrastructure.Audit;
using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using NovaCore.BuildingBlock.Infrastructure.Extensions;
using NovaCore.BuildingBlock.Infrastructure.Messaging;
using NovaCore.BuildingBlock.Messaging.Abstractions;
using NovaCore.BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NovaCore.User.Application.Abstractions.Services;
using NovaCore.User.Infrastructure.BackgroundJobs;
using NovaCore.User.Infrastructure.Caching.Roles;
using NovaCore.User.Infrastructure.Caching.Users;
using NovaCore.User.Infrastructure.GrpcClients;
using NovaCore.User.Infrastructure.Messaging.Consumers;

namespace NovaCore.User.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAppLogger()
            .AddRedisCache(configuration)
            .AddIdempotency(configuration)
            .AddScoped<IRoleCacheReader, RoleCacheReader>()
            .AddScoped<IUserProfileDetailCache, UserProfileDetailCache>()
            .AddBackgroundJobs(configuration)
            .AddInboxOutboxCleanupJobs(configuration)
            .AddHttpAuditMetadataProvider("User");

        // Register application event dispatcher (MediatR - for internal events)
        services.AddApplicationEventDispatcher();

        // Consumers must be registered before AddKafkaMessaging - their Topics are
        // discovered eagerly to configure the KafkaFlow consumer pipeline.
        services.AddMessagingConsumers();
        services.AddKafkaMessaging(configuration, "user-service");
        services.AddInboxOutboxInfrastructure(configuration);

        services.AddGrpcClients(configuration);

        return services;
    }

    private static IServiceCollection AddGrpcClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authServiceUrl = configuration["Grpc:AuthService:Url"] ?? "http://auth-api:5002";

        // Registering Auth gRPC service
        services.AddGrpcClient<AuthGrpcService.AuthGrpcServiceClient>(new Uri(authServiceUrl));
        services.AddScoped<IAuthClientService, AuthClientService>();

        return services;
    }

    private static IServiceCollection AddMessagingConsumers(
        this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventConsumer, UserAccountDeletionIntegrationEventConsumer>();
        services.AddScoped<IIntegrationEventConsumer, UserProfileCreatedSearchSyncConsumer>();
        services.AddScoped<IIntegrationEventConsumer, UserProfileUpdatedSearchSyncConsumer>();

        return services;
    }
}
