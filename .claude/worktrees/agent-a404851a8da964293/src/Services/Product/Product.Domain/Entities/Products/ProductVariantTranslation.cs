using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Products;

/// <summary>
/// Owned child of Product - a locale-specific override of the product's display copy.
/// </summary>
public sealed class ProductVariantTranslation : BaseEntity<Guid>, ITenantEntity
{
    public ProductVariant Variant { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string ShortDescription { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? SeoTitle { get; private set; }
    public string? SeoDescription { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductVariantTranslation() { }

    internal static ProductVariantTranslation Create(
        Guid variantId,
        LanguageCode languageCode,
        string name,
        string shortDescription,
        string description,
        string? seoTitle = null,
        string? seoDescription = null)
    {
        ValidateName(name);

        return new ProductVariantTranslation
        {
            Id = variantId,
            LanguageCode = languageCode,
            Name = name,
            ShortDescription = shortDescription,
            Description = description,
            SeoTitle = seoTitle,
            SeoDescription = seoDescription,
        };
    }

    public void UpdateContent(
        string name,
        string shortDescription,
        string description)
    {
        ValidateName(name);

        Name = name;
        ShortDescription = shortDescription;
        Description = description;
    }

    public void UpdateSeo(string? seoTitle, string? seoDescription)
    {
        SeoTitle = seoTitle;
        SeoDescription = seoDescription;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Translated name cannot be empty.");
    }
}
