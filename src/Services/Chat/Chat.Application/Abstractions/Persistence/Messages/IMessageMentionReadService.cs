using NovaCore.Chat.Application.Features.Interactions.Queries.GetMyInteractions;

namespace NovaCore.Chat.Application.Abstractions.Persistence.Messages;

public interface IMessageMentionReadService
{
    /// <summary>Mentions of the given user, most recent first, optionally strictly before a cursor timestamp.</summary>
    Task<IReadOnlyList<MyInteractionDto>> GetForUserAsync(Guid userId, long? beforeTicks, int take, CancellationToken ct = default);
}
