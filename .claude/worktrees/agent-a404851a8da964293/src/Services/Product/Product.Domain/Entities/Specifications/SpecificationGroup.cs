using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Specifications;

/// <summary>
/// Independent, reusable catalog lookup - a system-managed section (e.g. "Display", "Performance")
/// that groups related <see cref="SpecificationDefinition"/> fields for display on the product page.
/// Sellers never create groups or definitions; they only supply values via ProductSpecification.
/// </summary>
public sealed class SpecificationGroup : AggregateRoot<Guid>, IAuditable
{
    public string Code { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public CatalogStatus Status { get; private set; } = CatalogStatus.Active;

    public ICollection<SpecificationGroupTranslation> Translations { get; private set; } = [];
    public ICollection<SpecificationDefinition> SpecificationDefinitions { get; private set; } = [];

    private SpecificationGroup() { }

    public static SpecificationGroup Create(
        Guid id,
        string code,
        int displayOrder = 0,
        CatalogStatus status = CatalogStatus.Active)
    {
        ValidateCode(code);

        return new SpecificationGroup
        {
            Id = id,
            Code = code,
            DisplayOrder = displayOrder,
            Status = status,
        };
    }

    // ============================================================================
    // Translations
    // Manages the per-language display name/description for this group, one
    // entry per language code.
    // ============================================================================

    #region Translations

    public void AddTranslation(LanguageCode languageCode, string name, string? description = null)
    {
        if (Translations.Any(t => t.LanguageCode == languageCode))
            throw ExceptionFactory.Duplicate($"A translation for language '{languageCode}' already exists.");

        Translations.Add(SpecificationGroupTranslation.Create(Id, languageCode, name, description));
    }

    public void RemoveTranslation(LanguageCode languageCode)
    {
        var translation = Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (translation is null)
            return;

        Translations.Remove(translation);
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Display ordering and Active/Inactive status transitions, plus the shared
    // code-validation rule.
    // ============================================================================

    #region Details & lifecycle

    public void UpdateDisplayOrder(int displayOrder)
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

    public static bool IsValidCode(string? code) => !string.IsNullOrWhiteSpace(code);

    private static void ValidateCode(string code)
    {
        if (!IsValidCode(code))
            throw ExceptionFactory.RequiredField("Specification group code cannot be empty.");
    }

    #endregion
}
