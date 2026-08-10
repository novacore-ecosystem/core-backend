using NovaCore.BuildingBlock.Domain.Metadata;

namespace NovaCore.Product.Domain.Metadata;

/// <summary>Extensible, strongly-typed metadata shared across every variation of a Product.</summary>
public sealed class ProductMetadata : MetadataBase
{
    [Metadata("badge")]
    public string? Badge
    {
        get => Get<string>("badge");
        set => Set(value, "badge");
    }

    [Metadata("show_on_homepage", DefaultValue = false)]
    public bool ShowOnHomepage
    {
        get => Get<bool>("showOnHomePage");
        set => Set(value, "showOnHomePage");
    }

    [Metadata("seo_title")]
    public string? SeoTitle
    {
        get => Get<string>("seoTitle");
        set => Set(value, "seoTitle");
    }
}
