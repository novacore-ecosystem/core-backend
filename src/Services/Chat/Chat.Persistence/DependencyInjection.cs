using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Extensions;
using NovaCore.BuildingBlock.Persistence.Audit;
using NovaCore.BuildingBlock.Persistence.Ef.DependencyInjection;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;
using NovaCore.BuildingBlock.Persistence.Repository;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using OpenTelemetry.Trace;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationAssignments;
using NovaCore.Chat.Application.Abstractions.Persistence.ConversationPermissions;
using NovaCore.Chat.Application.Abstractions.Persistence.ConversationQueues;
using NovaCore.Chat.Application.Abstractions.Persistence.ConversationResponsibilityHistories;
using NovaCore.Chat.Application.Abstractions.Persistence.ConversationRoles;
using NovaCore.Chat.Application.Abstractions.Persistence.ConversationSchedules;
using NovaCore.Chat.Application.Abstractions.Persistence.ConversationTasks;
using NovaCore.Chat.Application.Abstractions.Persistence.ConversationTransferRequests;
using NovaCore.Chat.Application.Abstractions.Persistence.Contacts;
using NovaCore.Chat.Application.Abstractions.Persistence.ConversationRatings;
using NovaCore.Chat.Application.Abstractions.Persistence.ConversationReasonSuggestions;
using NovaCore.Chat.Application.Abstractions.Persistence.Conversations;
using NovaCore.Chat.Application.Abstractions.Persistence.Messages;
using NovaCore.Chat.Application.Abstractions.Persistence.Polls;
using NovaCore.Chat.Application.Abstractions.Persistence.Stickers;
using NovaCore.Chat.Application.Abstractions.Persistence.Tags;

using NovaCore.Chat.Persistence.Contexts.ConversationAssignments.Read;
using NovaCore.Chat.Persistence.Contexts.ConversationAssignments.Write;
using NovaCore.Chat.Persistence.Contexts.ConversationPermissions.Read;
using NovaCore.Chat.Persistence.Contexts.ConversationPermissions.Write;
using NovaCore.Chat.Persistence.Contexts.ConversationQueues.Read;
using NovaCore.Chat.Persistence.Contexts.ConversationQueues.Write;
using NovaCore.Chat.Persistence.Contexts.ConversationResponsibilityHistories.Read;
using NovaCore.Chat.Persistence.Contexts.ConversationResponsibilityHistories.Write;
using NovaCore.Chat.Persistence.Contexts.ConversationRoles.Read;
using NovaCore.Chat.Persistence.Contexts.ConversationRoles.Write;
using NovaCore.Chat.Persistence.Contexts.ConversationSchedules.Read;
using NovaCore.Chat.Persistence.Contexts.ConversationSchedules.Write;
using NovaCore.Chat.Persistence.Contexts.ConversationTasks.Read;
using NovaCore.Chat.Persistence.Contexts.ConversationTasks.Write;
using NovaCore.Chat.Persistence.Contexts.ConversationTransferRequests.Read;
using NovaCore.Chat.Persistence.Contexts.ConversationTransferRequests.Write;
using NovaCore.Chat.Persistence.Contexts.ConversationRatings.Read;
using NovaCore.Chat.Persistence.Contexts.ConversationRatings.Write;
using NovaCore.Chat.Persistence.Contexts.ConversationReasonSuggestions.Read;
using NovaCore.Chat.Persistence.Contexts.ConversationReasonSuggestions.Write;
using NovaCore.Chat.Persistence.Contexts.Contacts.Read;
using NovaCore.Chat.Persistence.Contexts.Contacts.Write;
using NovaCore.Chat.Persistence.Contexts.Conversations.Read;
using NovaCore.Chat.Persistence.Contexts.Conversations.Write;
using NovaCore.Chat.Persistence.Contexts.Messages.Read;
using NovaCore.Chat.Persistence.Contexts.Messages.Write;
using NovaCore.Chat.Persistence.Contexts.Polls.Read;
using NovaCore.Chat.Persistence.Contexts.Polls.Write;
using NovaCore.Chat.Persistence.Contexts.Stickers.Read;
using NovaCore.Chat.Persistence.Contexts.Stickers.Write;
using NovaCore.Chat.Persistence.Contexts.Tags.Read;
using NovaCore.Chat.Persistence.Contexts.Tags.Write;
using NovaCore.Chat.Persistence.Engine;
using NovaCore.Chat.Persistence.Engine.UnitOfWork;
using NovaCore.Chat.Persistence.Reliability.Inbox;
using NovaCore.Chat.Persistence.Reliability.Outbox;

namespace NovaCore.Chat.Persistence;

public static class DependencyInjection
{
    public static TracerProviderBuilder AddPersistenceTracing(this TracerProviderBuilder builder)
    {
        return builder.AddNpgsql();
    }

    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddDatabaseContext(configuration)
            .AddApplicationServices()
            .AddRepositories()
            .AddUnitOfWork()
            .AddOutbox()
            .AddInbox()
            .AddAuditHierarchy();

        return services;
    }

    // Only the 14 aggregate roots and their IAuditable owned children (Notes, Attachments,
    // References, Mentions, Options, Translations) are registered - pure mapping entities
    // (ConversationParticipant, MessageReaction, PollVote, ...) aren't IAuditable and stay
    // unregistered, same reasoning Product.Persistence documents for its own mapping entities.
    private static IServiceCollection AddAuditHierarchy(this IServiceCollection services)
    {
        services.ConfigureAuditHierarchy(builder =>
        {
            builder.Entity<Conversation>().IsRoot(x => x.Id);
            builder.Entity<ConversationNote>().BelongsTo<Conversation>(x => x.ConversationId);

            builder.Entity<Contact>().IsRoot(x => x.Id);

            builder.Entity<ConversationQueue>().IsRoot(x => x.Id);
            builder.Entity<ConversationAssignment>().IsRoot(x => x.Id);
            builder.Entity<ConversationTransferRequest>().IsRoot(x => x.Id);
            builder.Entity<ConversationResponsibilityHistory>().IsRoot(x => x.Id);

            builder.Entity<Tag>().IsRoot(x => x.Id);
            builder.Entity<TagTranslation>().BelongsTo<Tag>(x => x.Id);

            builder.Entity<ConversationReasonSuggestion>().IsRoot(x => x.Id);
            builder.Entity<ConversationReasonSuggestionTranslation>().BelongsTo<ConversationReasonSuggestion>(x => x.Id);

            builder.Entity<ConversationRating>().IsRoot(x => x.Id);

            builder.Entity<Message>().IsRoot(x => x.Id);
            builder.Entity<MessageAttachment>().BelongsTo<Message>(x => x.MessageId);
            builder.Entity<MessageReference>().BelongsTo<Message>(x => x.MessageId);
            builder.Entity<MessageMention>().BelongsTo<Message>(x => x.MessageId);

            builder.Entity<Poll>().IsRoot(x => x.Id);
            builder.Entity<PollOption>().BelongsTo<Poll>(x => x.PollId);

            builder.Entity<ConversationTask>().IsRoot(x => x.Id);
            builder.Entity<ConversationSchedule>().IsRoot(x => x.Id);

            builder.Entity<ConversationRole>().IsRoot(x => x.Id);
            builder.Entity<ConversationRoleTranslation>().BelongsTo<ConversationRole>(x => x.Id);

            builder.Entity<ConversationPermission>().IsRoot(x => x.Id);
            builder.Entity<ConversationPermissionTranslation>().BelongsTo<ConversationPermission>(x => x.Id);

            builder.Entity<Sticker>().IsRoot(x => x.Id);
            builder.Entity<StickerTranslation>().BelongsTo<Sticker>(x => x.Id);
        });

        return services;
    }

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddPersistenceDbContext<ChatDbContext>(connectionString);

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScopedByInterfaceAndConcrete<IAppService>(typeof(ChatDbContext));
        return services;
    }

    // Every <Aggregate>Repo implements the generic IRepository<T>/IRepository<T,TId> - Scrutor's
    // AsImplementedInterfaces() registers each concrete class against every interface it
    // implements (including the empty per-aggregate marker interfaces), so this one scan call
    // covers all 14. Read/Write services are registered explicitly since they're one-per-aggregate.
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScopedByInterface(typeof(IRepository<>), typeof(ChatDbContext));

        services.AddScoped<IConversationReadService, ConversationReadService>();
        services.AddScoped<IConversationWriteService, ConversationWriteService>();

        services.AddScoped<IContactReadService, ContactReadService>();
        services.AddScoped<IContactWriteService, ContactWriteService>();

        services.AddScoped<IConversationQueueReadService, ConversationQueueReadService>();
        services.AddScoped<IConversationQueueWriteService, ConversationQueueWriteService>();

        services.AddScoped<IConversationQueueItemReadService, ConversationQueueItemReadService>();
        services.AddScoped<IConversationQueueItemWriteService, ConversationQueueItemWriteService>();

        services.AddScoped<IConversationAssignmentReadService, ConversationAssignmentReadService>();
        services.AddScoped<IConversationAssignmentWriteService, ConversationAssignmentWriteService>();

        services.AddScoped<IConversationTransferRequestReadService, ConversationTransferRequestReadService>();
        services.AddScoped<IConversationTransferRequestWriteService, ConversationTransferRequestWriteService>();

        services.AddScoped<IConversationResponsibilityHistoryReadService, ConversationResponsibilityHistoryReadService>();
        services.AddScoped<IConversationResponsibilityHistoryWriteService, ConversationResponsibilityHistoryWriteService>();

        services.AddScoped<ITagReadService, TagReadService>();
        services.AddScoped<ITagWriteService, TagWriteService>();

        services.AddScoped<IConversationReasonSuggestionReadService, ConversationReasonSuggestionReadService>();
        services.AddScoped<IConversationReasonSuggestionWriteService, ConversationReasonSuggestionWriteService>();

        services.AddScoped<IConversationRatingReadService, ConversationRatingReadService>();
        services.AddScoped<IConversationRatingWriteService, ConversationRatingWriteService>();

        services.AddScoped<IMessageReadService, MessageReadService>();
        services.AddScoped<IMessageWriteService, MessageWriteService>();

        services.AddScoped<IPollReadService, PollReadService>();
        services.AddScoped<IPollWriteService, PollWriteService>();

        services.AddScoped<IConversationTaskReadService, ConversationTaskReadService>();
        services.AddScoped<IConversationTaskWriteService, ConversationTaskWriteService>();

        services.AddScoped<IConversationScheduleReadService, ConversationScheduleReadService>();
        services.AddScoped<IConversationScheduleWriteService, ConversationScheduleWriteService>();

        services.AddScoped<IConversationRoleReadService, ConversationRoleReadService>();
        services.AddScoped<IConversationRoleWriteService, ConversationRoleWriteService>();

        services.AddScoped<IConversationPermissionReadService, ConversationPermissionReadService>();
        services.AddScoped<IConversationPermissionWriteService, ConversationPermissionWriteService>();

        services.AddScoped<IStickerReadService, StickerReadService>();
        services.AddScoped<IStickerWriteService, StickerWriteService>();

        return services;
    }

    private static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    private static IServiceCollection AddOutbox(this IServiceCollection services)
    {
        services.AddEfOutboxStore<ChatDbContext>();
        services.AddScoped<IOutboxStore, OutboxStore>();

        return services;
    }

    private static IServiceCollection AddInbox(this IServiceCollection services)
    {
        services.AddEfInboxStore<ChatDbContext>();
        services.AddEfDeadLetterQueryService<ChatDbContext>();
        services.AddScoped<IInboxStore, InboxStore>();

        return services;
    }
}
