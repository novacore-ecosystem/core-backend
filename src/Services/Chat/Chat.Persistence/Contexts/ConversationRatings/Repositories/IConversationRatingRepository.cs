using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Chat.Persistence.Contexts.ConversationRatings.Repositories;

public interface IConversationRatingRepository : IRepository<ConversationRating, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
