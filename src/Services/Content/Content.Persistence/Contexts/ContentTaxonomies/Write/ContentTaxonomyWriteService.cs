using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using NovaCore.Content.Application.Abstractions.Persistence.Taxonomies;
using NovaCore.Content.Persistence.Contexts.ContentTaxonomies.Repositories;

namespace NovaCore.Content.Persistence.Contexts.ContentTaxonomies.Write;

public sealed class ContentTaxonomyWriteService(
    IContentTaxonomyRepository taxonomyRepo,
    IUnitOfWork unitOfWork) : IContentTaxonomyWriteService
{
    public async Task CreateAsync(ContentTaxonomy taxonomy, CancellationToken ct = default)
    {
        await taxonomyRepo.AddAsync(taxonomy, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
