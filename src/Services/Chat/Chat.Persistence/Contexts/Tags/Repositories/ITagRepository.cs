using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Chat.Persistence.Contexts.Tags.Repositories;

public interface ITagRepository : IRepository<Tag, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
