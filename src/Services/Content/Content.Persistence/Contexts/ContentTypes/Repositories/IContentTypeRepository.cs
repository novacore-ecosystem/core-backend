using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Content.Persistence.Contexts.ContentTypes.Repositories;

public interface IContentTypeRepository : IRepository<ContentType, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
