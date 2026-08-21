using NovaCore.Chat.Persistence.Contexts;
using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts.Tags.Repositories;

public sealed class TagRepo(ChatDbContext dbContext)
    : ChatBaseRepository<Tag, Guid>(dbContext), ITagRepository
{
}
