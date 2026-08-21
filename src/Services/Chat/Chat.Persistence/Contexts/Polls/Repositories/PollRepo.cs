using NovaCore.Chat.Persistence.Contexts;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.Polls.Repositories;

public sealed class PollRepo(ChatDbContext dbContext)
    : ChatBaseRepository<Poll, Guid>(dbContext), IPollRepository
{
}
