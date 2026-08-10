using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Infrastructure.BackgroundJobs;
using NovaCore.Auth.Infrastructure.Caching;
using NovaCore.Auth.Infrastructure.GrpcClients;
using NovaCore.Auth.Infrastructure.Messaging.Consumers;
using NovaCore.Auth.Infrastructure.Security;
using NovaCore.Auth.Infrastructure.Services;

using NovaCore.BuildingBlock.Contract.Protos.User;
using NovaCore.BuildingBlock.Grpc.Client;
using NovaCore.BuildingBlock.Infrastructure.Audit;
using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using NovaCore.BuildingBlock.Infrastructure.Extensions;
using NovaCore.BuildingBlock.Infrastructure.Messaging;
using NovaCore.BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAppLogger()
            .AddRedisCache(configuration)
            .AddRoleCaching(configuration)
            .AddBackgroundJobs(configuration)
            .AddInboxOutboxCleanupJobs(configuration)
            .AddHttpAuditMetadataProvider("Auth")
            .AddSecurityServices()
            .AddApplicationEventDispatcher()
            .AddMessagingConsumers()
            .AddKafkaMessaging(configuration, "auth-service")
            .AddInboxOutboxInfrastructure(configuration)
            .AddGrpcClients(configuration)
            .AddApplicationServices();

        return services;
    }

    private static IServiceCollection AddMessagingConsumers(
        this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventConsumer, UserCreatedIntegrationEventConsumer>();

        return services;
    }

    private static IServiceCollection AddRoleCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<RoleCacheService>();

        // Decorate IAuthService with caching. This must be called AFTER
        // Persistence.AddPersistence which registers the original AuthService
        services.AddScoped<IAuthService>(provider =>
        {
            var innerAuthService = provider.GetRequiredService<AuthService>();
            var roleCache = provider.GetRequiredService<RoleCacheService>();
            return new CachedAuthServiceDecorator(innerAuthService, roleCache);
        });

        return services;
    }

    private static IServiceCollection AddGrpcClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var userServiceUrl = configuration["Grpc:UserService:Url"] ?? "http://user-api:5002";

        services.AddGrpcClient<UserGrpcService.UserGrpcServiceClient>(new Uri(userServiceUrl));
        services.AddScoped<IUserProfileService, UserProfileServiceClient>();

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScopedByInterface<IAppService>(typeof(DependencyInjection));
        return services;
    }
}
