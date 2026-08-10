using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Collections;

/// <summary>
/// Independent, reusable merchandising collection (e.g. "Summer Sale", "New Arrivals"). A Product
/// can belong to multiple collections via <see cref="Products.ProductCollectionMapping"/>.
/// </summary>
public sealed class ProductCollection : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Slug Slug { get; private set; } = null!;
    public Guid? BannerMediaId { get; private set; }
    public CollectionType CollectionType { get; private set; }
    public DateTime? StartAt { get; private set; }
    public DateTime? EndAt { get; private set; }
    public int DisplayOrder { get; private set; }
    public CatalogStatus Status { get; private set; } = CatalogStatus.Active;

    public ICollection<ProductCollectionTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductCollection() { }

    public static ProductCollection Create(
        Guid id,
        string name,
        string description,
        Slug slug,
        CollectionType collectionType,
        Guid? bannerMediaId = null,
        DateTime? startAt = null,
        DateTime? endAt = null,
        int displayOrder = 0,
        CatalogStatus status = CatalogStatus.Active)
    {
        ValidateName(name);

        if (startAt is not null && endAt is not null && startAt > endAt)
            throw ExceptionFactory.InvalidRange("Collection StartAt cannot be later than EndAt.");

        return new ProductCollection
        {
            Id = id,
            Name = name,
            Description = description,
            Slug = slug,
            CollectionType = collectionType,
            BannerMediaId = bannerMediaId,
            StartAt = startAt,
            EndAt = endAt,
            DisplayOrder = displayOrder,
            Status = status,
        };
    }

    // ============================================================================
    // Translations
    // Upserts the per-language name/description override for this collection,
    // one entry per language code.
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
        }
        else
        {
            var translation = ProductCollectionTranslation.Create(
                Id,
                languageCode,
                name,
                description);
            Translations.Add(translation);
        }
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Core descriptive fields (name, description, slug, banner), the
    // scheduling window, display ordering, and Active/Inactive status
    // transitions, plus the shared name-validation rule.
    // ============================================================================

    #region Details & lifecycle

    public void UpdateDetails(string name, string description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public void ChangeSlug(Slug slug)
    {
        Slug = slug;
    }

    public void ChangeBanner(Guid? bannerMediaId)
    {
        BannerMediaId = bannerMediaId;
    }

    public void Schedule(DateTime? startAt, DateTime? endAt)
    {
        if (startAt is not null && endAt is not null && startAt > endAt)
            throw ExceptionFactory.InvalidRange("Collection StartAt cannot be later than EndAt.");

        StartAt = startAt;
        EndAt = endAt;
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
            throw ExceptionFactory.RequiredField("Collection name cannot be empty.");
    }

    #endregion
}
