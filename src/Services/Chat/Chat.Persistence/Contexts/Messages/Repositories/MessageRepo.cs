using NovaCore.Chat.Persistence.Contexts;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.Messages.Repositories;

public sealed class MessageRepo(ChatDbContext dbContext)
    : ChatBaseRepository<Message, Guid>(dbContext), IMessageRepository
{
}
