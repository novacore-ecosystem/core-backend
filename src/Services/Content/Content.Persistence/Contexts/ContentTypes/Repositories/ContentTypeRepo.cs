using NovaCore.Content.Persistence.Contexts;
using NovaCore.Content.Persistence.Engine;

namespace NovaCore.Content.Persistence.Contexts.ContentTypes.Repositories;

public sealed class ContentTypeRepo(ContentDbContext dbContext)
    : ContentBaseRepository<ContentType, Guid>(dbContext), IContentTypeRepository
{
}
