using NovaCore.Content.Application.Abstractions.Persistence.Contents;
using NovaCore.Content.Persistence.Contexts.Contents.Repositories;
using NovaCore.Content.Persistence.Engine;

namespace NovaCore.Content.Persistence.Contexts.Contents.Read;

public sealed class ContentReadService(IContentRepository contentRepo, ContentDbContext dbContext) : IContentReadService
{
    public async Task<ContentEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await contentRepo.GetByIdAsync(
            id,
            query => query
                .Include(c => c.ContentType)
                .Include(c => c.Versions).ThenInclude(v => v.Localizations)
                .Include(c => c.Publications)
                .Include(c => c.WorkflowInstances)
                .Include(c => c.Relationships)
                .Include(c => c.TaxonomyAssignments).ThenInclude(a => a.Taxonomy)
                .Include(c => c.Audiences)
                .Include(c => c.Contributors),
            ct);
    }

    public async Task<ContentEntity?> GetBySlugAsync(ContentSlug slug, CancellationToken ct = default)
    {
        return await dbContext.Contents
            .AsNoTracking()
            .Include(c => c.ContentType)
            .Include(c => c.Versions)
            .FirstOrDefaultAsync(c => c.Slug == slug, ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await contentRepo.ExistsByIdAsync(id, ct);
    }

    public async Task<bool> ExistsBySlugAsync(ContentSlug slug, CancellationToken ct = default)
    {
        return await dbContext.Contents.AsNoTracking().AnyAsync(c => c.Slug == slug, ct);
    }
}
