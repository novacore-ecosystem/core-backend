using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Chat.Application.Abstractions.Persistence.Messages;

namespace NovaCore.Chat.Application.Features.Interactions.Queries.GetMyInteractions;

/// <summary>
/// Merges two independently-ordered streams (mentions of the caller, reactions on the caller's
/// messages) into one feed. Each stream is queried for up to Limit+1 of its own most-recent items
/// (bounded, so the merge stays cheap even though it happens in memory rather than a DB-level
/// UNION across two differently-shaped tables), then the top Limit+1 across both determine the
/// page and whether there's a next one.
/// </summary>
public sealed class GetMyInteractionsHandler(
    IMessageMentionReadService mentionReadService,
    IMessageReactionReadService reactionReadService,
    ICurrentUserService currentUser) : IQueryHandler<GetMyInteractionsQuery, CursorPaginatedResult<MyInteractionDto>>
{
    public async Task<CursorPaginatedResult<MyInteractionDto>> Handle(GetMyInteractionsQuery request, CancellationToken ct = default)
    {
        var callerId = currentUser.GetUserId() ?? throw new UnauthorizedException();
        var hasCursor = InteractionCursor.TryDecode(request.Cursor, out var beforeTicks);
        var take = request.Limit + 1;

        var mentions = await mentionReadService.GetForUserAsync(callerId, hasCursor ? beforeTicks : null, take, ct);
        var reactions = await reactionReadService.GetOnUserMessagesAsync(callerId, hasCursor ? beforeTicks : null, take, ct);

        var merged = mentions
            .Concat(reactions)
            .OrderByDescending(x => x.OccurredAt)
            .Take(take)
            .ToList();

        var hasMore = merged.Count > request.Limit;
        var page = hasMore ? merged.Take(request.Limit).ToList() : merged;
        var nextCursor = hasMore ? InteractionCursor.Encode(page[^1].OccurredAt) : null;

        return CursorPaginatedResult<MyInteractionDto>.Create(page, nextCursor);
    }
}
