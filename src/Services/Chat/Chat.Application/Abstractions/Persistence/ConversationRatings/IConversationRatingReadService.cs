namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationRatings;

public interface IConversationRatingReadService
{
    Task<ConversationRating?> GetByConversationIdAsync(Guid conversationId, CancellationToken ct = default);
}
