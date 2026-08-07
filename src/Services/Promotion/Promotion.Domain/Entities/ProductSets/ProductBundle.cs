namespace NovaCore.Promotion.Domain.Entities.ProductSets;

/// <summary>
/// A priced bundling arrangement of a ProductSet's items - BundlePrice/BundleRule/BundleGift
/// reference it by BundleId only, no Navigation section was given for this entity either. Unlike
/// every other translatable type in this roadmap, ProductBundle is not an Aggregate Root - it
/// exposes its own Translate(...) directly (reachable via ProductSet.Bundles, same as its other
/// public members), since only ProductSet's own Translate() is the "one per Aggregate Root" rule
/// this Phase 2.5 review enforces.
/// </summary>
public sealed class ProductBundle : BaseEntity<Guid>, IAuditable
{
    public Guid ProductSetId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }

    public ProductSet ProductSet { get; private set; } = default!;
    public ICollection<BundlePrice> Prices { get; private set; } = [];
    public ICollection<BundleRule> Rules { get; private set; } = [];
    public ICollection<BundleGift> Gifts { get; private set; } = [];
    public ICollection<ProductBundleTranslation> Translations { get; private set; } = [];

    private ProductBundle() { }

    /// <summary>Only ProductSet may construct a ProductBundle - see ProductSet.AddBundle.</summary>
    internal static ProductBundle Create(Guid productSetId, string name, int displayOrder)
    {
        ValidateName(name);

        return new ProductBundle
        {
            Id = Guid.CreateVersion7(),
            ProductSetId = productSetId,
            Name = name,
            DisplayOrder = displayOrder,
        };
    }

    #region Translations
    public void Translate(LanguageCode languageCode, string name)
    {
        ValidateName(name);

        var existing = Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is not null)
        {
            existing.UpdateDetails(name);
            return;
        }

        Translations.Add(ProductBundleTranslation.Create(Id, languageCode, name));
    }
    #endregion

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Bundle name cannot be empty.");
    }
}
