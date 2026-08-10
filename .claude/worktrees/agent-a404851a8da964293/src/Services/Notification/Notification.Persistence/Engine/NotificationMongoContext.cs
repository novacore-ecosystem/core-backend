using NovaCore.BuildingBlock.Persistence.Mongo.Inbox;
using NovaCore.BuildingBlock.Persistence.Mongo.MongoContext;
using NovaCore.BuildingBlock.Persistence.Mongo.Outbox;

namespace NovaCore.Notification.Persistence.Engine;

/// <summary>
/// One collection per aggregate root - NotificationRuleTarget/NotificationCampaignTarget are not
/// separate collections since they're owned children, embedded as native nested documents inside
/// their parent's Targets array (same pattern as Audit's AuditTrailNode). Collection names are
/// pre-provisioned by scripts/mongodb/init-mongo.js - do not rename without updating that script.
/// </summary>
public sealed class NotificationMongoContext : MongoContextBase, IOutboxMongoContext, IInboxMongoContext
{
    public IMongoCollection<UserNotification> UserNotifications { get; }
    public IMongoCollection<NotificationGroup> NotificationGroups { get; }
    public IMongoCollection<NotificationTemplate> NotificationTemplates { get; }
    public IMongoCollection<NotificationRule> NotificationRules { get; }
    public IMongoCollection<NotificationCampaign> NotificationCampaigns { get; }
    public IMongoCollection<NotificationChannel> NotificationChannels { get; }
    public IMongoCollection<NotificationDispatch> NotificationDispatches { get; }
    public IMongoCollection<OutboxDocument> OutboxMessages { get; }
    public IMongoCollection<InboxDocument> InboxMessages { get; }
    public IMongoCollection<InboxRetryHistoryDocument> InboxRetryHistories { get; }

    public NotificationMongoContext(IMongoDatabase database) : base(database)
    {
        UserNotifications = database.GetCollection<UserNotification>("user_notifications");
        NotificationGroups = database.GetCollection<NotificationGroup>("notification_groups");
        NotificationTemplates = database.GetCollection<NotificationTemplate>("notification_templates");
        NotificationRules = database.GetCollection<NotificationRule>("notification_rules");
        NotificationCampaigns = database.GetCollection<NotificationCampaign>("notification_campaigns");
        NotificationChannels = database.GetCollection<NotificationChannel>("notification_channels");
        NotificationDispatches = database.GetCollection<NotificationDispatch>("notification_dispatches");
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
