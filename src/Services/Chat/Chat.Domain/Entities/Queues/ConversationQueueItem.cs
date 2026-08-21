namespace NovaCore.Chat.Domain.Entities.Queues;

/// <summary>
/// A conversation's current placement in a ConversationQueue - composite key (QueueId,
/// ConversationId), one row per conversation-in-queue. Related to Conversation by id only (no
/// Conversation navigation) - the Conversation aggregate does not own its own queue placement.
/// Atomic claim/locking on Assigned belongs to Application/Infrastructure, not here.
/// </summary>
public sealed class ConversationQueueItem : BaseEntity, ITenantEntity
{
    public Guid QueueId { get; private set; }
    public ConversationQueue Queue { get; private set; } = default!;
    public Guid ConversationId { get; private set; }
    public DateTime EnqueuedAt { get; private set; }
    public int Priority { get; private set; }
    public ConversationQueueItemStatus Status { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ConversationQueueItem() { }

    public static ConversationQueueItem Create(Guid queueId, Guid conversationId, int priority = 0)
    {
        return new ConversationQueueItem
        {
            QueueId = queueId,
            ConversationId = conversationId,
            EnqueuedAt = DateTime.UtcNow,
            Priority = priority,
            Status = ConversationQueueItemStatus.Waiting,
        };
    }

    public void MarkAssigned()
    {
        if (Status != ConversationQueueItemStatus.Waiting)
            throw ExceptionFactory.InvalidStatus($"Cannot assign a queue item in {Status} status.");

        Status = ConversationQueueItemStatus.Assigned;
    }

    public void MarkRemoved()
    {
        Status = ConversationQueueItemStatus.Removed;
    }
}
