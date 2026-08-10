using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Brands;

/// <summary>
/// Independent, reusable catalog lookup.
/// Products reference a brand via Product.BrandId.
/// </summary>
public sealed class ProductBrand : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Slug Slug { get; private set; } = null!;
    public Guid? LogoMediaId { get; private set; }
    public string? Website { get; private set; }
    public string? Country { get; private set; }
    public bool IsFeatured { get; private set; }
    public int DisplayOrder { get; private set; }
    public string Note { get; private set; } = string.Empty;
    public CatalogStatus Status { get; private set; } = CatalogStatus.Active;
    public ICollection<ProductBrandTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductBrand() { }

    public static ProductBrand Create(
        Guid id,
        string name,
        string description,
        Slug slug,
        Guid? logoMediaId = null,
        string? website = null,
        string? country = null,
        bool isFeatured = false,
        int displayOrder = 0,
        CatalogStatus status = CatalogStatus.Active,
        string note = "")
    {
        ValidateName(name);

        return new ProductBrand
        {
            Id = id,
            Name = name,
            Description = description,
            Slug = slug,
            LogoMediaId = logoMediaId,
            Website = website,
            Country = country,
            IsFeatured = isFeatured,
            DisplayOrder = displayOrder,
            Status = status,
            Note = note,
        };
    }

    // ============================================================================
    // Translations
    // Manages the per-language name/description/note override for this
    // brand, upserting by language code.
    // ============================================================================

    #region Translations

    public void Translate(
        LanguageCode languageCode,
        string name,
        string description)
    {
        ValidateName(name);

        var existingTranslation = Translations
            .FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existingTranslation != null)
        {
            existingTranslation.UpdateDetails(name, description);
            return;
        }

        var translation = ProductBrandTranslation.Create(
            Id,
            languageCode,
            name,
            description);

        Translations.Add(translation);
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Core descriptive fields, slug/logo assignment, featured flag, display
    // ordering, and Active/Inactive status transitions, plus the shared
    // name-validation rule.
    // ============================================================================

    #region Details & lifecycle

    internal void UpdateDetails(
        string name,
        string description,
        string? website,
        string? country,
        string note)
    {
        ValidateName(name);

        Name = name;
        Description = description;
        Website = website;
        Country = country;
        Note = note;
    }

    public void ChangeSlug(Slug slug)
    {
        Slug = slug;
    }

    public void ChangeLogo(Guid? logoMediaId)
    {
        LogoMediaId = logoMediaId;
    }

    public void Feature()
    {
        IsFeatured = true;
    }

    public void Unfeature()
    {
        IsFeatured = false;
    }

    public void ChangeDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    public void Activate()
    {
        Status = CatalogStatus.Active;
    }

    public void Deactivate()
    {
        Status = CatalogStatus.Inactive;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Brand name cannot be empty.");
    }

    #endregion
}
