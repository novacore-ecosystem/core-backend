using NovaCore.Chat.Application.Features.Interactions.Queries.GetMyInteractions;

namespace NovaCore.Chat.Application.Abstractions.Persistence.Messages;

public interface IMessageReactionReadService
{
    /// <summary>Reactions left on messages the given user sent, most recent first, optionally strictly before a cursor timestamp.</summary>
    Task<IReadOnlyList<MyInteractionDto>> GetOnUserMessagesAsync(Guid userId, long? beforeTicks, int take, CancellationToken ct = default);
}
