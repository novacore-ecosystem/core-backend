using MongoDB.Driver;

namespace NovaCore.BuildingBlock.Persistence.Mongo.Outbox;

/// <summary>
/// Marker interface for Mongo context implementations that provide access to the Outbox collection.
/// Used by MongoOutboxStore to remain generic across all service Mongo context types.
/// </summary>
public interface IOutboxMongoContext
{
    IMongoCollection<OutboxDocument> OutboxMessages { get; }
}
