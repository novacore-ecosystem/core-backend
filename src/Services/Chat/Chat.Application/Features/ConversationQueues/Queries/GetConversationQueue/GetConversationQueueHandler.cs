using NovaCore.BuildingBlock.Application.Abstractions.Common;

using NovaCore.Chat.Application.Abstractions.Persistence.ConversationQueues;

namespace NovaCore.Chat.Application.Features.ConversationQueues.Queries.GetConversationQueue;

public sealed class GetConversationQueueHandler(IConversationQueueItemReadService queueItemReadService)
    : IQueryHandler<GetConversationQueueQuery, CursorPaginatedResult<ConversationQueueItemDto>>
{
    public async Task<CursorPaginatedResult<ConversationQueueItemDto>> Handle(
        GetConversationQueueQuery request,
        CancellationToken ct = default)
    {
        var hasCursor = QueueCursor.TryDecode(request.Cursor, out var afterTicks, out var afterConversationId);

        var items = await queueItemReadService.GetWaitingItemsAsync(
            request.QueueId,
            request.Limit + 1,
            hasCursor ? afterTicks : null,
            hasCursor ? afterConversationId : null,
            ct);

        var hasMore = items.Count > request.Limit;
        var page = hasMore ? items.Take(request.Limit).ToList() : items;

        var nextCursor = hasMore
            ? QueueCursor.Encode(page[^1].EnqueuedAt, page[^1].ConversationId)
            : null;

        return CursorPaginatedResult<ConversationQueueItemDto>.Create(page, nextCursor);
    }
}
