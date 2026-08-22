namespace NovaCore.Content.Application.Abstractions.Persistence.Taxonomies;

public interface IContentTaxonomyReadService
{
    Task<ContentTaxonomy?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);
}
