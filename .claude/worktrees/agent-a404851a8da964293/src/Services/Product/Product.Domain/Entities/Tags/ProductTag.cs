using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Tags;

/// <summary>
/// Independent, reusable catalog lookup - flat, no hierarchy.
/// Products reference tags via ProductTagMapping.
/// </summary>
public sealed class ProductTag : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Slug Slug { get; private set; } = null!;
    public string? Color { get; private set; }
    public int DisplayOrder { get; private set; }
    public CatalogStatus Status { get; private set; } = CatalogStatus.Active;
    public string Note { get; private set; } = string.Empty;

    public ICollection<ProductTagTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductTag() { }

    public static ProductTag Create(
        Guid id,
        string name,
        string description,
        Slug slug,
        string? color = null,
        int displayOrder = 0,
        CatalogStatus status = CatalogStatus.Active,
        string note = "")
    {
        ValidateName(name);

        return new ProductTag
        {
            Id = id,
            Name = name,
            Description = description,
            Slug = slug,
            Color = color,
            DisplayOrder = displayOrder,
            Status = status,
            Note = note
        };
    }

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
            var translation = ProductTagTranslation.Create(
                Id,
                languageCode,
                name,
                description);
            Translations.Add(translation);
        }
    }

    public void Rename(string name)
    {
        ValidateName(name);

        Name = name;
    }

    public void UpdateDetails(string description, string? color)
    {
        Description = description;
        Color = color;
    }

    public void ChangeSlug(Slug slug)
    {
        Slug = slug;
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
            throw ExceptionFactory.RequiredField("Tag name cannot be empty.");
    }
}
