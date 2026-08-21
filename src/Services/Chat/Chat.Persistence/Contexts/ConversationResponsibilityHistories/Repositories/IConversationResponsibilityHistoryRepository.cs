using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Chat.Persistence.Contexts.ConversationResponsibilityHistories.Repositories;

public interface IConversationResponsibilityHistoryRepository : IRepository<ConversationResponsibilityHistory, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
