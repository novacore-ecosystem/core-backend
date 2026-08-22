using NovaCore.Content.Application.Abstractions.Persistence.ContentTypes;
using NovaCore.Content.Persistence.Contexts.ContentTypes.Repositories;
using NovaCore.Content.Persistence.Engine;

namespace NovaCore.Content.Persistence.Contexts.ContentTypes.Read;

public sealed class ContentTypeReadService(IContentTypeRepository contentTypeRepo, ContentDbContext dbContext) : IContentTypeReadService
{
    public async Task<ContentType?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await contentTypeRepo.GetByIdAsync(id, query => query.Include(t => t.FieldDefinitions), ct);
    }

    public async Task<ContentType?> GetByKeyAsync(ContentKey key, CancellationToken ct = default)
    {
        return await dbContext.ContentTypes.AsNoTracking().FirstOrDefaultAsync(t => t.Key == key, ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await contentTypeRepo.ExistsByIdAsync(id, ct);
    }
}
