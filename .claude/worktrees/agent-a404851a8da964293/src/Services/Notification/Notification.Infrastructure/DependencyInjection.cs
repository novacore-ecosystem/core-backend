using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using NovaCore.BuildingBlock.Infrastructure.Extensions;
using NovaCore.BuildingBlock.Infrastructure.Messaging;
using NovaCore.BuildingBlock.Messaging.Abstractions;
using NovaCore.BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NovaCore.Notification.Application.Abstractions.Services;
using NovaCore.Notification.Infrastructure.BackgroundJobs;
using NovaCore.Notification.Infrastructure.Caching;
using NovaCore.Notification.Infrastructure.Delivery;
using NovaCore.Notification.Infrastructure.Messaging.Consumers;
using NovaCore.Notification.Infrastructure.SignalR.Facade;
using NovaCore.Notification.Infrastructure.SignalR.Hubs.Global;
using NovaCore.Notification.Infrastructure.Workers;

namespace NovaCore.Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAppLogger()
            .AddBackgroundJobs(configuration)
            .AddInboxOutboxCleanupJobs(configuration)
            .AddDispatchWorker(configuration);

        // Register application event dispatcher (MediatR - for internal events)
        services.AddApplicationEventDispatcher();

        services.AddMessagingConsumers();
        services.AddKafkaMessaging(configuration, "notification-service");
        services.AddInboxOutboxInfrastructure(configuration);
        services.AddNotificationDelivery();
        services.AddNotificationChannelCache();

        return services;
    }

    private static IServiceCollection AddDispatchWorker(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<NotificationDispatchWorkerOptions>(configuration.GetSection(NotificationDispatchWorkerOptions.Section));
        return services;
    }

    private static IServiceCollection AddMessagingConsumers(this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventConsumer, NotificationTriggerConsumer>();
        return services;
    }

    private static IServiceCollection AddNotificationDelivery(this IServiceCollection services)
    {
        services.AddScoped<ActorHubFacade<GlobalHub, IGlobalHubClient, IGlobalHubClient>>();
        services.AddScoped<IChannelSender, SignalRChannelSender>();
        services.AddScoped<IChannelSenderResolver, ChannelSenderResolver>();
        services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();
        return services;
    }

    private static IServiceCollection AddNotificationChannelCache(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<INotificationChannelCache, NotificationChannelCache>();
        return services;
    }
}
