using NovaCore.BuildingBlock.Persistence.Mongo.Inbox;
using NovaCore.BuildingBlock.Persistence.Mongo.MongoContext;
using NovaCore.BuildingBlock.Persistence.Mongo.Outbox;

namespace NovaCore.Audit.Persistence.Engine;

public sealed class AuditMongoContext : MongoContextBase, IOutboxMongoContext, IInboxMongoContext
{
    // Collection name "logs" matches the pre-provisioned collection/indexes in
    // scripts/mongodb/init-mongo.js - do not rename without updating that script.
    public IMongoCollection<AuditLogEntry> AuditLogs { get; }
    public IMongoCollection<OutboxDocument> OutboxMessages { get; }
    public IMongoCollection<InboxDocument> InboxMessages { get; }
    public IMongoCollection<InboxRetryHistoryDocument> InboxRetryHistories { get; }

    public AuditMongoContext(IMongoDatabase database) : base(database)
    {
        AuditLogs = database.GetCollection<AuditLogEntry>("logs");
        OutboxMessages = database.GetCollection<OutboxDocument>("outbox_messages");
        InboxMessages = database.GetCollection<InboxDocument>("inbox_messages");
        InboxRetryHistories = database.GetCollection<InboxRetryHistoryDocument>("inbox_retry_histories");

        // No OnModelCreating equivalent in Mongo - index creation happens once here, since
        // this context is registered as a Singleton (see AddPersistenceMongoContext).
        OutboxMessages.EnsureOutboxIndexes();
        InboxMessages.EnsureInboxIndexes();
        InboxRetryHistories.EnsureInboxRetryHistoryIndexes();
    }
}
