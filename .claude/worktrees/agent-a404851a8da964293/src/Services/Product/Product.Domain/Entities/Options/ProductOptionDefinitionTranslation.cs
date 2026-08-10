using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Options;

/// <summary>
/// Owned child of ProductOptionDefinition - a locale-specific display name/description for the
/// shared option dimension. Composite-keyed by (ProductOptionDefinitionId, LanguageCode): one
/// entry per language, no independent identity.
/// </summary>
public sealed class ProductOptionDefinitionTranslation : BaseEntity, IAuditable
{
    public Guid ProductOptionDefinitionId { get; private set; }
    public ProductOptionDefinition ProductOptionDefinition { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private ProductOptionDefinitionTranslation() { }

    internal static ProductOptionDefinitionTranslation Create(
        Guid productOptionDefinitionId,
        LanguageCode languageCode,
        string name,
        string? description = null)
    {
        ValidateName(name);

        return new ProductOptionDefinitionTranslation
        {
            ProductOptionDefinitionId = productOptionDefinitionId,
            LanguageCode = languageCode,
            Name = name,
            Description = description,
        };
    }

    public void UpdateContent(string name, string? description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Translated name cannot be empty.");
    }
}
