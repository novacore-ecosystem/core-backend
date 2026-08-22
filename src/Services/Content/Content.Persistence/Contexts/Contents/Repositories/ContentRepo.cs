using NovaCore.Content.Persistence.Contexts;
using NovaCore.Content.Persistence.Engine;

namespace NovaCore.Content.Persistence.Contexts.Contents.Repositories;

public sealed class ContentRepo(ContentDbContext dbContext)
    : ContentBaseRepository<ContentEntity, Guid>(dbContext), IContentRepository
{
}
