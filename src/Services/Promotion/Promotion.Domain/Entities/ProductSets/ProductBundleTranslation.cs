namespace NovaCore.Promotion.Domain.Entities.ProductSets;

/// <summary>Per-language override of a ProductBundle's Name. Identity is BundleId + LanguageCode (Phase 3.1 correction) - no surrogate Id. Name-only, since ProductBundle itself has no Description field.</summary>
public sealed class ProductBundleTranslation : BaseEntity, IAuditable, ITenantEntity
{
    public Guid BundleId { get; private set; }
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;

    public ProductBundle Bundle { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductBundleTranslation() { }

    /// <summary>Only ProductBundle may construct a ProductBundleTranslation - see ProductBundle.Translate.</summary>
    internal static ProductBundleTranslation Create(Guid bundleId, LanguageCode languageCode, string name)
    {
        ValidateName(name);

        return new ProductBundleTranslation
        {
            BundleId = bundleId,
            LanguageCode = languageCode,
            Name = name,
        };
    }

    internal void UpdateDetails(string name)
    {
        ValidateName(name);

        Name = name;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Translated bundle name cannot be empty.");
    }
}
