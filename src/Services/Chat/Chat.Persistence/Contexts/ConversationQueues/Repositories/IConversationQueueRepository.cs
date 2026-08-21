using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Chat.Persistence.Contexts.ConversationQueues.Repositories;

public interface IConversationQueueRepository : IRepository<ConversationQueue, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
