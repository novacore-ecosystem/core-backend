using NovaCore.Auth.Application.Abstractions.Apps;
using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Registrations;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Infrastructure.Authorization;
using NovaCore.Auth.Infrastructure.BackgroundJobs;
using NovaCore.Auth.Infrastructure.Caching;
using NovaCore.Auth.Infrastructure.Caching.Apps;
using NovaCore.Auth.Infrastructure.Caching.Registrations;
using NovaCore.Auth.Infrastructure.Configurations;
using NovaCore.Auth.Infrastructure.Configurations.Settings;
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
            .AddAuthConfigurations(configuration)
            .AddAppLogger()
            .AddRedisCache(configuration)
            .AddAuthService()
            .AddTenantCaching()
            .AddAppCaching()
            .AddAccountAuthorization()
            .AddRegistrationDefaultsCaching()
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

    private static IServiceCollection AddAuthService(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    private static IServiceCollection AddTenantCaching(this IServiceCollection services)
    {
        services.AddScoped<ITenantVersionCache, TenantVersionCache>();

        return services;
    }

    private static IServiceCollection AddAppCaching(this IServiceCollection services)
    {
        services.AddScoped<IAppCollectionCache, AppCollectionCache>();
        services.AddScoped<IAppMembershipCache, AppMembershipCache>();

        return services;
    }

    private static IServiceCollection AddAccountAuthorization(this IServiceCollection services)
    {
        services.AddScoped<IEffectiveAuthorizationCache, EffectiveAuthorizationCache>();
        services.AddScoped<IAccountAuthorizationService, AccountAuthorizationService>();

        return services;
    }

    private static IServiceCollection AddRegistrationDefaultsCaching(this IServiceCollection services)
    {
        services.AddScoped<IRegistrationDefaultsCache, RegistrationDefaultsCache>();

        return services;
    }

    private static IServiceCollection AddGrpcClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var grpcSetting = configuration.GetSection(AuthGrpcSetting.Section).Get<AuthGrpcSetting>() ?? new AuthGrpcSetting();

        services.AddGrpcClient<UserGrpcService.UserGrpcServiceClient>(new Uri(grpcSetting.Url));
        services.AddScoped<IUserProfileService, UserProfileServiceClient>();

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScopedByInterface<IAppService>(typeof(DependencyInjection));
        return services;
    }
}
