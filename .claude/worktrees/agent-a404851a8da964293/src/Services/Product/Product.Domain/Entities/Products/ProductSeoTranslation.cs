using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Products;

/// <summary>
/// Owned child of Product - a locale-specific override of the product's SEO metadata.
/// </summary>
public sealed class ProductSeoTranslation : BaseEntity<Guid>, ITenantEntity
{
    public Product Product { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public string? MetaKeywords { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductSeoTranslation() { }

    public static ProductSeoTranslation Create(
        Guid productId,
        LanguageCode languageCode,
        string? metaTitle = null,
        string? metaDescription = null,
        string? metaKeywords = null)
    {
        return new ProductSeoTranslation
        {
            Id = productId,
            LanguageCode = languageCode,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            MetaKeywords = metaKeywords
        };
    }

    public void UpdateMeta(
        string? metaTitle,
        string? metaDescription,
        string? metaKeywords)
    {
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        MetaKeywords = metaKeywords;
    }
}
