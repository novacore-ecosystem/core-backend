using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Chat.Persistence.Contexts.Conversations.Repositories;

public interface IConversationRepository : IRepository<Conversation, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
