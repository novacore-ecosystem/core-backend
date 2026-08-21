using NovaCore.Chat.Application.Abstractions.Persistence.ConversationRatings;
using NovaCore.Chat.Persistence.Contexts.ConversationRatings.Repositories;

namespace NovaCore.Chat.Persistence.Contexts.ConversationRatings.Read;

public sealed class ConversationRatingReadService(IConversationRatingRepository ratingRepo) : IConversationRatingReadService
{
    public async Task<ConversationRating?> GetByConversationIdAsync(Guid conversationId, CancellationToken ct = default)
    {
        return await ratingRepo.GetAsync(r => r.ConversationId == conversationId, ct);
    }
}
