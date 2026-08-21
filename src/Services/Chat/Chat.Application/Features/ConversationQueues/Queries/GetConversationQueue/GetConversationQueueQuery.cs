using NovaCore.BuildingBlock.Application.Abstractions.Common;

namespace NovaCore.Chat.Application.Features.ConversationQueues.Queries.GetConversationQueue;

public sealed record GetConversationQueueQuery(
    Guid? QueueId,
    string? Cursor,
    int Limit = 20) : IQuery<CursorPaginatedResult<ConversationQueueItemDto>>;

public sealed record ConversationQueueItemDto(
    Guid ConversationId,
    Guid QueueId,
    DateTime EnqueuedAt,
    int Priority,
    string? Title,
    ConversationType Type,
    ConversationPriority ConversationPriority);
