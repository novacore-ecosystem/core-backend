using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Products;

/// <summary>
/// Owned child of Product, referenced back via Product.SeoId.
/// Split out of Product so SEO fields don't clutter the aggregate root when unused.
/// </summary>
public sealed class ProductSeo : BaseEntity<Guid>, ITenantEntity
{
    public Product Product { get; private set; } = default!;
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public string? MetaKeywords { get; private set; }
    public string? CanonicalUrl { get; private set; }
    public string? OgTitle { get; private set; }
    public string? OgDescription { get; private set; }
    public string? OgImage { get; private set; }
    public ICollection<ProductSeoTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductSeo() { }

    public static ProductSeo Create(
        Guid productId,
        string? metaTitle = null,
        string? metaDescription = null,
        string? metaKeywords = null,
        string? canonicalUrl = null,
        string? ogTitle = null,
        string? ogDescription = null,
        string? ogImage = null)
    {
        return new ProductSeo
        {
            Id = productId,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            MetaKeywords = metaKeywords,
            CanonicalUrl = canonicalUrl,
            OgTitle = ogTitle,
            OgDescription = ogDescription,
            OgImage = ogImage,
        };
    }

    // ============================================================================
    // Translations
    // Upserts the per-language meta/canonical/OpenGraph override for this SEO
    // profile, one entry per language code.
    // ============================================================================

    #region Translations

    public void TranslateSeo(
        LanguageCode languageCode,
        string? metaTitle = null,
        string? metaDescription = null,
        string? metaKeywords = null)
    {
        var translation = Translations
            .FirstOrDefault(t => t.LanguageCode == languageCode);
        if (translation is not null)
        {
            translation.UpdateMeta(metaTitle, metaDescription, metaKeywords);
            return;
        }

        translation = ProductSeoTranslation.Create(
            productId: Id,
            languageCode: languageCode,
            metaTitle: metaTitle,
            metaDescription: metaDescription,
            metaKeywords: metaKeywords);
        Translations.Add(translation);
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Core meta tags, canonical URL, and OpenGraph fields for the default
    // (untranslated) SEO profile.
    // ============================================================================

    #region Details & lifecycle

    public void UpdateMeta(
        string? metaTitle,
        string? metaDescription,
        string? metaKeywords)
    {
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        MetaKeywords = metaKeywords;
    }

    public void UpdateCanonicalUrl(string? canonicalUrl)
    {
        CanonicalUrl = canonicalUrl;
    }

    public void UpdateOpenGraph(
        string? ogTitle,
        string? ogDescription,
        string? ogImage)
    {
        OgTitle = ogTitle;
        OgDescription = ogDescription;
        OgImage = ogImage;
    }

    #endregion
}
