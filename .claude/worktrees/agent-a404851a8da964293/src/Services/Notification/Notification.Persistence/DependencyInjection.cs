using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Persistence.Mongo.DependencyInjection;
using NovaCore.BuildingBlock.Persistence.Mongo.Inbox;
using NovaCore.BuildingBlock.Persistence.Mongo.Outbox;
using NovaCore.BuildingBlock.Persistence.Mongo.Serialization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NovaCore.Notification.Application.Abstractions.Persistence.NotificationCampaigns;
using NovaCore.Notification.Application.Abstractions.Persistence.NotificationChannels;
using NovaCore.Notification.Application.Abstractions.Persistence.NotificationDispatches;
using NovaCore.Notification.Application.Abstractions.Persistence.NotificationGroups;
using NovaCore.Notification.Application.Abstractions.Persistence.NotificationRules;
using NovaCore.Notification.Application.Abstractions.Persistence.NotificationTemplates;
using NovaCore.Notification.Application.Abstractions.Persistence.UserNotifications;
using NovaCore.Notification.Domain.ValueObjects;
using NovaCore.Notification.Persistence.Contexts.NotificationCampaigns.Read;
using NovaCore.Notification.Persistence.Contexts.NotificationCampaigns.Repositories;
using NovaCore.Notification.Persistence.Contexts.NotificationCampaigns.Write;
using NovaCore.Notification.Persistence.Contexts.NotificationChannels.Read;
using NovaCore.Notification.Persistence.Contexts.NotificationChannels.Repositories;
using NovaCore.Notification.Persistence.Contexts.NotificationChannels.Write;
using NovaCore.Notification.Persistence.Contexts.NotificationDispatches.Read;
using NovaCore.Notification.Persistence.Contexts.NotificationDispatches.Repositories;
using NovaCore.Notification.Persistence.Contexts.NotificationDispatches.Write;
using NovaCore.Notification.Persistence.Contexts.NotificationGroups.Read;
using NovaCore.Notification.Persistence.Contexts.NotificationGroups.Repositories;
using NovaCore.Notification.Persistence.Contexts.NotificationGroups.Write;
using NovaCore.Notification.Persistence.Contexts.NotificationRules.Read;
using NovaCore.Notification.Persistence.Contexts.NotificationRules.Repositories;
using NovaCore.Notification.Persistence.Contexts.NotificationRules.Write;
using NovaCore.Notification.Persistence.Contexts.NotificationTemplates.Read;
using NovaCore.Notification.Persistence.Contexts.NotificationTemplates.Repositories;
using NovaCore.Notification.Persistence.Contexts.NotificationTemplates.Write;
using NovaCore.Notification.Persistence.Contexts.UserNotifications.Read;
using NovaCore.Notification.Persistence.Contexts.UserNotifications.Repositories;
using NovaCore.Notification.Persistence.Contexts.UserNotifications.Write;
using NovaCore.Notification.Persistence.Engine;
using NovaCore.Notification.Persistence.Engine.UnitOfWork;
using NovaCore.Notification.Persistence.Reliability.Inbox;
using NovaCore.Notification.Persistence.Reliability.Outbox;

namespace NovaCore.Notification.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        RegisterValueObjectClassMaps();

        services
            .AddDatabaseContext(configuration)
            .AddRepositories()
            .AddUnitOfWork()
            .AddOutboxAndInbox();

        return services;
    }

    /// <summary>
    /// Every get-only-property value object in NovaCore.Notification.Domain needs an explicit BsonClassMap
    /// or it silently round-trips as an empty subdocument - see BsonImmutableValueObjectRegistrar
    /// and docs/tasks/2026-07-22/Task2_notification-list-null-fields.md for why.
    /// </summary>
    private static void RegisterValueObjectClassMaps()
    {
        BsonImmutableValueObjectRegistrar.Register<NotificationCategory>("Value");
        BsonImmutableValueObjectRegistrar.Register<NotificationType>("Value");
        BsonImmutableValueObjectRegistrar.Register<NotificationContent>("Title", "Body");
        BsonImmutableValueObjectRegistrar.Register<AudienceSelector>("Type", "ConfigJson");
        BsonImmutableValueObjectRegistrar.Register<ChannelConfiguration>("ConfigJson");
        BsonImmutableValueObjectRegistrar.Register<DispatchReference>("ReferenceType", "ReferenceId");
        BsonImmutableValueObjectRegistrar.Register<NotificationSchedule>("ExecutionType", "StartAt", "EndAt", "CronExpression");
        BsonImmutableValueObjectRegistrar.Register<TemplateContent>("Subject", "Body", "Variables");
    }

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Mongo")
            ?? throw new InvalidOperationException("Connection string 'Mongo' not found.");
        var databaseName = configuration["Mongo:DatabaseName"]
            ?? throw new InvalidOperationException("Configuration 'Mongo:DatabaseName' not found.");

        services.AddPersistenceMongoContext<NotificationMongoContext>(connectionString, databaseName);

        return services;
    }

    // No generic IRepository<T> to Scrutor-scan for here - Mongo repos have no DbContext/
    // IQueryable shape to satisfy it, same reasoning as Audit's IAuditLogRepository. Registered
    // explicitly rather than discovered, one per aggregate root (Repository Design: repositories
    // are designed only around aggregate roots, never per child entity).
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserNotificationRepository, UserNotificationRepo>();
        services.AddScoped<IUserNotificationReadService, UserNotificationReadService>();
        services.AddScoped<IUserNotificationWriteService, UserNotificationWriteService>();

        services.AddScoped<INotificationGroupRepository, NotificationGroupRepo>();
        services.AddScoped<INotificationGroupReadService, NotificationGroupReadService>();
        services.AddScoped<INotificationGroupWriteService, NotificationGroupWriteService>();

        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepo>();
        services.AddScoped<INotificationTemplateReadService, NotificationTemplateReadService>();
        services.AddScoped<INotificationTemplateWriteService, NotificationTemplateWriteService>();

        services.AddScoped<INotificationRuleRepository, NotificationRuleRepo>();
        services.AddScoped<INotificationRuleReadService, NotificationRuleReadService>();
        services.AddScoped<INotificationRuleWriteService, NotificationRuleWriteService>();

        services.AddScoped<INotificationCampaignRepository, NotificationCampaignRepo>();
        services.AddScoped<INotificationCampaignReadService, NotificationCampaignReadService>();
        services.AddScoped<INotificationCampaignWriteService, NotificationCampaignWriteService>();

        services.AddScoped<INotificationChannelRepository, NotificationChannelRepo>();
        services.AddScoped<INotificationChannelReadService, NotificationChannelReadService>();
        services.AddScoped<INotificationChannelWriteService, NotificationChannelWriteService>();

        services.AddScoped<INotificationDispatchRepository, NotificationDispatchRepo>();
        services.AddScoped<INotificationDispatchReadService, NotificationDispatchReadService>();
        services.AddScoped<INotificationDispatchWriteService, NotificationDispatchWriteService>();

        return services;
    }

    private static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    private static IServiceCollection AddOutboxAndInbox(this IServiceCollection services)
    {
        services
            .AddMongoOutboxStore<NotificationMongoContext>()
            .AddMongoInboxStore<NotificationMongoContext>()
            .AddMongoDeadLetterQueryService<NotificationMongoContext>();

        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IInboxStore, InboxStore>();

        return services;
    }
}
