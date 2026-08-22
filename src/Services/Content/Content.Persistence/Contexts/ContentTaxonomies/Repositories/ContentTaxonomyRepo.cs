using NovaCore.Content.Persistence.Contexts;
using NovaCore.Content.Persistence.Engine;

namespace NovaCore.Content.Persistence.Contexts.ContentTaxonomies.Repositories;

public sealed class ContentTaxonomyRepo(ContentDbContext dbContext)
    : ContentBaseRepository<ContentTaxonomy, Guid>(dbContext), IContentTaxonomyRepository
{
}
