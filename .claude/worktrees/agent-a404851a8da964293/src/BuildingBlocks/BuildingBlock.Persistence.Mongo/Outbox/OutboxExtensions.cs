using NovaCore.BuildingBlock.Persistence.Mongo.MongoContext;
using NovaCore.BuildingBlock.Persistence.Outbox;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Driver;

namespace NovaCore.BuildingBlock.Persistence.Mongo.Outbox;

public static class OutboxExtensions
{
    /// <summary>
    /// Ensure the Outbox collection's indexes exist. Call once from the derived Mongo context's
    /// constructor - Mongo has no OnModelCreating equivalent to apply configuration declaratively.
    /// </summary>
    public static void EnsureOutboxIndexes(this IMongoCollection<OutboxDocument> collection)
    {
        collection.Indexes.CreateOne(new CreateIndexModel<OutboxDocument>(
            Builders<OutboxDocument>.IndexKeys.Ascending(x => x.ProcessedAt),
            new CreateIndexOptions { Name = "idx_outbox_processed_at" }));
    }

    /// <summary>
    /// Register the generic Mongo outbox store for the given Mongo context type.
    /// The context must implement IOutboxMongoContext.
    /// </summary>
    public static IServiceCollection AddMongoOutboxStore<TContext>(this IServiceCollection services)
        where TContext : MongoContextBase, IOutboxMongoContext
    {
        services.AddScoped<IOutboxStore, MongoOutboxStore<TContext>>();
        return services;
    }
}
