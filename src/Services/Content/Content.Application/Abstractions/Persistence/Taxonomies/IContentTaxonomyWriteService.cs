namespace NovaCore.Content.Application.Abstractions.Persistence.Taxonomies;

public interface IContentTaxonomyWriteService
{
    /// <summary>Self-commits (bare SaveChangesAsync) - a brand-new taxonomy node is a single-aggregate write.</summary>
    Task CreateAsync(ContentTaxonomy taxonomy, CancellationToken ct = default);
}
