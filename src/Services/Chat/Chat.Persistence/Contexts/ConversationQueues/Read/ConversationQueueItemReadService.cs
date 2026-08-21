using NovaCore.Chat.Application.Abstractions.Persistence.ConversationQueues;
using NovaCore.Chat.Application.Features.ConversationQueues.Queries.GetConversationQueue;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.ConversationQueues.Read;

/// <summary>
/// ConversationQueueItem is a pure-mapping, composite-key entity with no Conversation navigation
/// and no internal Create gate (see ConversationQueueItem.cs) - it cannot be reached through the
/// generic IRepository&lt;T,TId&gt; binding, same reasoning Promotion.Persistence documents for
/// PromotionExclusion. Queries ChatDbContext directly instead of through a repository.
/// </summary>
public sealed class ConversationQueueItemReadService(ChatDbContext dbContext) : IConversationQueueItemReadService
{
    public async Task<IReadOnlyList<ConversationQueueItemDto>> GetWaitingItemsAsync(
        Guid? queueId,
        int take,
        long? afterEnqueuedAtTicks,
        Guid? afterConversationId,
        CancellationToken ct = default)
    {
        var query = dbContext.ConversationQueueItems
            .AsNoTracking()
            .Where(i => i.Status == ConversationQueueItemStatus.Waiting);

        if (queueId is not null)
            query = query.Where(i => i.QueueId == queueId);

        if (afterEnqueuedAtTicks is not null && afterConversationId is not null)
        {
            var afterEnqueuedAt = new DateTime(afterEnqueuedAtTicks.Value, DateTimeKind.Utc);
            query = query.Where(i =>
                i.EnqueuedAt > afterEnqueuedAt ||
                (i.EnqueuedAt == afterEnqueuedAt && i.ConversationId.CompareTo(afterConversationId.Value) > 0));
        }

        return await query
            .OrderBy(i => i.EnqueuedAt).ThenBy(i => i.ConversationId)
            .Take(take)
            .Join(dbContext.Conversations.AsNoTracking(),
                item => item.ConversationId,
                conversation => conversation.Id,
                (item, conversation) => new ConversationQueueItemDto(
                    item.ConversationId,
                    item.QueueId,
                    item.EnqueuedAt,
                    item.Priority,
                    conversation.Title,
                    conversation.Type,
                    conversation.Priority))
            .ToListAsync(ct);
    }
}
