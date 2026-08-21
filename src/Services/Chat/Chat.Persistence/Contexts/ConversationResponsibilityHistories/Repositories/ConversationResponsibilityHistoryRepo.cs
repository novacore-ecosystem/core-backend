using NovaCore.Chat.Persistence.Contexts;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.ConversationResponsibilityHistories.Repositories;

public sealed class ConversationResponsibilityHistoryRepo(ChatDbContext dbContext)
    : ChatBaseRepository<ConversationResponsibilityHistory, Guid>(dbContext), IConversationResponsibilityHistoryRepository
{
}
