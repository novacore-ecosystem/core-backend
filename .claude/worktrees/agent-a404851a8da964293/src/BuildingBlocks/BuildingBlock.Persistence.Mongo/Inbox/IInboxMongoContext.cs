using MongoDB.Driver;

namespace NovaCore.BuildingBlock.Persistence.Mongo.Inbox;

/// <summary>
/// Marker interface for Mongo context implementations that provide access to the Inbox collection.
/// Used by MongoInboxStore to remain generic across all service Mongo context types.
/// </summary>
public interface IInboxMongoContext
{
    IMongoCollection<InboxDocument> InboxMessages { get; }
    IMongoCollection<InboxRetryHistoryDocument> InboxRetryHistories { get; }
}
