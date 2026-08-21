using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Chat.Persistence.Contexts.Polls.Repositories;

public interface IPollRepository : IRepository<Poll, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
