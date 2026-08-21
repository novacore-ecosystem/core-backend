using NovaCore.Chat.Persistence.Contexts;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.ConversationRatings.Repositories;

public sealed class ConversationRatingRepo(ChatDbContext dbContext)
    : ChatBaseRepository<ConversationRating, Guid>(dbContext), IConversationRatingRepository
{
}
