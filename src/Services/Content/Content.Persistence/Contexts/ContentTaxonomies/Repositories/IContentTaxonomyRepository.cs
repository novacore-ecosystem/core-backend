using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Content.Persistence.Contexts.ContentTaxonomies.Repositories;

public interface IContentTaxonomyRepository : IRepository<ContentTaxonomy, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
