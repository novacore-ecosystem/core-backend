namespace NovaCore.Chat.Application.Abstractions.Persistence.ConversationRatings;

public interface IConversationRatingWriteService
{
    /// <summary>Commits via bare SaveChangesAsync.</summary>
    Task CreateAsync(ConversationRating rating, CancellationToken ct = default);
}
