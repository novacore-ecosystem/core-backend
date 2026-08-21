using NovaCore.Chat.Persistence.Contexts;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.ConversationTasks.Repositories;

public sealed class ConversationTaskRepo(ChatDbContext dbContext)
    : ChatBaseRepository<ConversationTask, Guid>(dbContext), IConversationTaskRepository
{
}
