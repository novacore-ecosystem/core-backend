using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Content.Persistence.Contexts.Contents.Repositories;

public interface IContentRepository : IRepository<ContentEntity, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
