using NovaCore.BuildingBlock.Application.Abstractions.DeadLetters;
using NovaCore.BuildingBlock.Persistence.Inbox;
using NovaCore.BuildingBlock.Persistence.Mongo.MongoContext;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Driver;

namespace NovaCore.BuildingBlock.Persistence.Mongo.Inbox;

public static class InboxExtensions
{
    /// <summary>
    /// Ensure the Inbox collection's indexes exist. Call once from the derived Mongo context's
    /// constructor - Mongo has no OnModelCreating equivalent to apply configuration declaratively.
    /// </summary>
    public static void EnsureInboxIndexes(this IMongoCollection<InboxDocument> collection)
    {
        collection.Indexes.CreateOne(new CreateIndexModel<InboxDocument>(
            Builders<InboxDocument>.IndexKeys.Ascending(x => x.MessageId).Ascending(x => x.ConsumerName),
            new CreateIndexOptions { Name = "idx_inbox_message_consumer_unique", Unique = true }));

        collection.Indexes.CreateOne(new CreateIndexModel<InboxDocument>(
            Builders<InboxDocument>.IndexKeys.Ascending(x => x.ProcessedAt),
            new CreateIndexOptions { Name = "idx_inbox_processed_at" }));

        // Covers the InboxRetryHostedService poll: WHERE Status = Retrying AND NextRetryAt <= now.
        collection.Indexes.CreateOne(new CreateIndexModel<InboxDocument>(
            Builders<InboxDocument>.IndexKeys.Ascending(x => x.Status).Ascending(x => x.NextRetryAt),
            new CreateIndexOptions { Name = "idx_inbox_status_next_retry_at" }));
    }

    /// <summary>
    /// Ensure the InboxRetryHistory collection's indexes exist. Call once from the derived Mongo
    /// context's constructor alongside EnsureInboxIndexes.
    /// </summary>
    public static void EnsureInboxRetryHistoryIndexes(this IMongoCollection<InboxRetryHistoryDocument> collection)
    {
        // Lists a row's retry history most-recent-first (GetRetryHistoryAsync).
        collection.Indexes.CreateOne(new CreateIndexModel<InboxRetryHistoryDocument>(
            Builders<InboxRetryHistoryDocument>.IndexKeys.Ascending(x => x.InboxMessageId).Descending(x => x.StartedAt),
            new CreateIndexOptions { Name = "idx_inbox_retry_history_message_started_at" }));

        // Finds the single open (FinishedAt == null) entry closed out by CompleteAttemptAsync/FailAttemptAsync.
        collection.Indexes.CreateOne(new CreateIndexModel<InboxRetryHistoryDocument>(
            Builders<InboxRetryHistoryDocument>.IndexKeys.Ascending(x => x.InboxMessageId).Ascending(x => x.FinishedAt),
            new CreateIndexOptions { Name = "idx_inbox_retry_history_message_open" }));
    }

    /// <summary>
    /// Register the generic Mongo inbox store for the given Mongo context type.
    /// The context must implement IInboxMongoContext.
    /// </summary>
    public static IServiceCollection AddMongoInboxStore<TContext>(this IServiceCollection services)
        where TContext : MongoContextBase, IInboxMongoContext
    {
        services.AddScoped<IInboxStore, MongoInboxStore<TContext>>();
        return services;
    }

    /// <summary>
    /// Register the generic Mongo dead-letter query service for the given Mongo context type.
    /// </summary>
    public static IServiceCollection AddMongoDeadLetterQueryService<TContext>(this IServiceCollection services)
        where TContext : MongoContextBase, IInboxMongoContext
    {
        services.AddScoped<IDeadLetterQueryService, MongoDeadLetterQueryService<TContext>>();
        return services;
    }
}
