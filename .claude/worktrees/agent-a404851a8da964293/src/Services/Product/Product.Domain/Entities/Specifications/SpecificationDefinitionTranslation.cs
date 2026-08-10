using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Specifications;

/// <summary>
/// Owned child of SpecificationDefinition - a locale-specific display name/description for the
/// field. Composite-keyed by (SpecificationDefinitionId, LanguageCode): one entry per language.
/// </summary>
public sealed class SpecificationDefinitionTranslation : BaseEntity<Guid>, IAuditable
{
    public Guid SpecificationDefinitionId { get; private set; }
    public SpecificationDefinition SpecificationDefinition { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private SpecificationDefinitionTranslation() { }

    internal static SpecificationDefinitionTranslation Create(
        Guid specificationDefinitionId,
        LanguageCode languageCode,
        string name,
        string? description = null)
    {
        ValidateName(name);

        return new SpecificationDefinitionTranslation
        {
            Id = Guid.CreateVersion7(),
            SpecificationDefinitionId = specificationDefinitionId,
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
