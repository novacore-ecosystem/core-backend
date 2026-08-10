using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Collections;

/// <summary>
/// Owned child of ProductCollection - a locale-specific override of the collection's display copy.
/// </summary>
public sealed class ProductCollectionTranslation : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public ProductCollection Collection { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductCollectionTranslation() { }

    public static ProductCollectionTranslation Create(
        Guid id,
        LanguageCode languageCode,
        string name,
        string description)
    {
        ValidateName(name);

        return new ProductCollectionTranslation
        {
            Id = id,
            LanguageCode = languageCode,
            Name = name,
            Description = description,
        };
    }

    public void UpdateDetails(string name, string description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Collection name cannot be empty.");
    }
}
