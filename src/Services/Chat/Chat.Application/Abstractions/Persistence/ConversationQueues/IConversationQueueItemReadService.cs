using NovaCore.Chat.Application.Features.ConversationQueues.Queries.GetConversationQueue;

namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationQueues;

public interface IConversationQueueItemReadService
{
    /// <summary>
    /// Waiting items ordered by (EnqueuedAt, ConversationId) - the keyset the cursor is built on.
    /// Requests take+1 rows so the caller can tell whether there's a next page without a separate
    /// count query.
    /// </summary>
    Task<IReadOnlyList<ConversationQueueItemDto>> GetWaitingItemsAsync(
        Guid? queueId,
        int take,
        long? afterEnqueuedAtTicks,
        Guid? afterConversationId,
        CancellationToken ct = default);
}
