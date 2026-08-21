using NovaCore.BuildingBlock.Application.Abstractions.Common;

namespace NovaCore.Chat.Application.Features.Interactions.Queries.GetMyInteractions;

public sealed record GetMyInteractionsQuery(string? Cursor, int Limit = 20)
    : IQuery<CursorPaginatedResult<MyInteractionDto>>;

public enum InteractionType
{
    Mention = 1,
    Reaction = 2,
}

/// <summary>
/// One unified, cursor-paginated feed: someone mentioned the caller, or someone reacted to a
/// message the caller sent. MentionType/ReactionType are populated for the matching Type only.
/// </summary>
public sealed record MyInteractionDto(
    InteractionType Type,
    Guid ConversationId,
    Guid MessageId,
    Guid? ActorUserId,
    DateTime OccurredAt,
    MentionType? MentionType,
    ReactionType? ReactionType);
