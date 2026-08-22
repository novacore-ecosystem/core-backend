using NovaCore.BuildingBlock.Domain.Metadata;

namespace NovaCore.Content.Domain.Metadata;

/// <summary>
/// Extensible, strongly-typed metadata carried by a ContentVersion. Covers the generic
/// SEO/presentation fields the Content Platform needs to expose; anything more product-specific
/// (structured SEO rules, canonical URL policy, etc.) belongs to WCM/SEO capabilities built on top
/// of this, not here - see MetadataBase for the underlying extensible key/value mechanism that
/// lets a caller stash additional custom keys beyond the typed properties below.
/// </summary>
public sealed class ContentMetadata : MetadataBase
{
    [Metadata("seo_title")]
    public string? SeoTitle
    {
        get => Get<string>("seoTitle");
        set => Set(value, "seoTitle");
    }

    [Metadata("meta_description")]
    public string? MetaDescription
    {
        get => Get<string>("metaDescription");
        set => Set(value, "metaDescription");
    }

    [Metadata("canonical_url")]
    public string? CanonicalUrl
    {
        get => Get<string>("canonicalUrl");
        set => Set(value, "canonicalUrl");
    }

    [Metadata("featured_image_reference")]
    public string? FeaturedImageReference
    {
        get => Get<string>("featuredImageReference");
        set => Set(value, "featuredImageReference");
    }

    [Metadata("excerpt")]
    public string? Excerpt
    {
        get => Get<string>("excerpt");
        set => Set(value, "excerpt");
    }
}
