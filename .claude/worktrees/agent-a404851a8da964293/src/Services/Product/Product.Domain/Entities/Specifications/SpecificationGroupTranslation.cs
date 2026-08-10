using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Specifications;

/// <summary>
/// Owned child of SpecificationGroup - a locale-specific display name/description for the group.
/// Composite-keyed by (SpecificationGroupId, LanguageCode): one entry per language.
/// </summary>
public sealed class SpecificationGroupTranslation : BaseEntity<Guid>, IAuditable
{
    public Guid SpecificationGroupId { get; private set; }
    public SpecificationGroup SpecificationGroup { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private SpecificationGroupTranslation() { }

    internal static SpecificationGroupTranslation Create(
        Guid specificationGroupId,
        LanguageCode languageCode,
        string name,
        string? description = null)
    {
        ValidateName(name);

        return new SpecificationGroupTranslation
        {
            Id = Guid.CreateVersion7(),
            SpecificationGroupId = specificationGroupId,
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
