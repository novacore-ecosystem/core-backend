using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Chat.Persistence.Contexts.ConversationReasonSuggestions.Repositories;

public interface IConversationReasonSuggestionRepository : IRepository<ConversationReasonSuggestion, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
