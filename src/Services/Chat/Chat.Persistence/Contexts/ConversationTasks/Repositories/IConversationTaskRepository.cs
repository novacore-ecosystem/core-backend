using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Chat.Persistence.Contexts.ConversationTasks.Repositories;

public interface IConversationTaskRepository : IRepository<ConversationTask, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
