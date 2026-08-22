namespace NovaCore.Content.Domain.Entities.Contents;

/// <summary>
/// Many-to-many mapping between Content and ContentTaxonomy. A pure existence-mapping (no history
/// beyond the pairing itself), so per domain-coding-conventions it extends the non-generic
/// BaseEntity with no surrogate Id - the composite key is (ContentId, TaxonomyId).
/// </summary>
public sealed class ContentTaxonomyAssignment : BaseEntity, ITenantEntity
{
    public Guid ContentId { get; private set; }
    public Content Content { get; private set; } = default!;
    public Guid TaxonomyId { get; private set; }
    public ContentTaxonomy Taxonomy { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ContentTaxonomyAssignment() { }

    internal static ContentTaxonomyAssignment Create(Guid contentId, Guid taxonomyId)
    {
        return new ContentTaxonomyAssignment
        {
            ContentId = contentId,
            TaxonomyId = taxonomyId,
        };
    }
}
