using NovaCore.Chat.Persistence.Contexts;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.ConversationTransferRequests.Repositories;

public sealed class ConversationTransferRequestRepo(ChatDbContext dbContext)
    : ChatBaseRepository<ConversationTransferRequest, Guid>(dbContext), IConversationTransferRequestRepository
{
}
