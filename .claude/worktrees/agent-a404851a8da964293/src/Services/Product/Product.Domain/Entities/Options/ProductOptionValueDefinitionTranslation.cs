using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Options;

/// <summary>
/// Owned child of ProductOptionValueDefinition - a locale-specific display name for the reusable
/// value. Composite-keyed by (ProductOptionValueDefinitionId, LanguageCode): one entry per
/// language, no independent identity.
/// </summary>
public sealed class ProductOptionValueDefinitionTranslation : BaseEntity, IAuditable
{
    public Guid ProductOptionValueDefinitionId { get; private set; }
    public ProductOptionValueDefinition ProductOptionValueDefinition { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;

    private ProductOptionValueDefinitionTranslation() { }

    internal static ProductOptionValueDefinitionTranslation Create(
        Guid productOptionValueDefinitionId,
        LanguageCode languageCode,
        string name)
    {
        ValidateName(name);

        return new ProductOptionValueDefinitionTranslation
        {
            ProductOptionValueDefinitionId = productOptionValueDefinitionId,
            LanguageCode = languageCode,
            Name = name,
        };
    }

    public void UpdateName(string name)
    {
        ValidateName(name);
        Name = name;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Translated name cannot be empty.");
    }
}
