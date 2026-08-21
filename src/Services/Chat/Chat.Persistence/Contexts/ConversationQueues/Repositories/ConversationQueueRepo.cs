using NovaCore.Chat.Persistence.Contexts;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.ConversationQueues.Repositories;

public sealed class ConversationQueueRepo(ChatDbContext dbContext)
    : ChatBaseRepository<ConversationQueue, Guid>(dbContext), IConversationQueueRepository
{
}
