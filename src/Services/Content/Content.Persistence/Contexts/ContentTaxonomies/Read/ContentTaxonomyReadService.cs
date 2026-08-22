using NovaCore.Content.Application.Abstractions.Persistence.Taxonomies;
using NovaCore.Content.Persistence.Contexts.ContentTaxonomies.Repositories;

namespace NovaCore.Content.Persistence.Contexts.ContentTaxonomies.Read;

public sealed class ContentTaxonomyReadService(IContentTaxonomyRepository taxonomyRepo) : IContentTaxonomyReadService
{
    public async Task<ContentTaxonomy?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await taxonomyRepo.GetByIdAsync(id, query => query.Include(t => t.Children), ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await taxonomyRepo.ExistsByIdAsync(id, ct);
    }
}
