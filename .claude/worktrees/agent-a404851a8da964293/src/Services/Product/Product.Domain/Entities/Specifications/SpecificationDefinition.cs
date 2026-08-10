using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Specifications;

/// <summary>
/// Independent, reusable catalog lookup - a system-managed specification field (e.g. "Screen Size",
/// "CPU", "Battery Capacity") belonging to a <see cref="SpecificationGroup"/>. Products reference a
/// definition via <see cref="Products.ProductSpecification.SpecificationDefinitionId"/> and only
/// supply the value; the field's name and data shape are owned entirely by this entity.
/// </summary>
public sealed class SpecificationDefinition : AggregateRoot<Guid>, IAuditable
{
    public Guid SpecificationGroupId { get; private set; }
    public SpecificationGroup Group { get; private set; } = default!;
    public string Code { get; private set; } = string.Empty;
    public AttributeDataType DataType { get; private set; }
    public string? DefaultUnit { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsRequired { get; private set; }
    public CatalogStatus Status { get; private set; } = CatalogStatus.Active;

    public ICollection<SpecificationDefinitionTranslation> Translations { get; private set; } = [];

    private SpecificationDefinition() { }

    public static SpecificationDefinition Create(
        Guid id,
        Guid specificationGroupId,
        string code,
        AttributeDataType dataType,
        string? defaultUnit = null,
        int displayOrder = 0,
        bool isRequired = false,
        CatalogStatus status = CatalogStatus.Active)
    {
        ValidateCode(code);

        return new SpecificationDefinition
        {
            Id = id,
            SpecificationGroupId = specificationGroupId,
            Code = code,
            DataType = dataType,
            DefaultUnit = defaultUnit,
            DisplayOrder = displayOrder,
            IsRequired = isRequired,
            Status = status,
        };
    }

    // ============================================================================
    // Translations
    // Manages the per-language display name/description for this definition,
    // one entry per language code.
    // ============================================================================

    #region Translations

    public void AddTranslation(LanguageCode languageCode, string name, string? description = null)
    {
        if (Translations.Any(t => t.LanguageCode == languageCode))
            throw ExceptionFactory.Duplicate($"A translation for language '{languageCode}' already exists.");

        Translations.Add(SpecificationDefinitionTranslation.Create(Id, languageCode, name, description));
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
    // Group membership, display ordering, default unit, and Active/Inactive
    // status transitions, plus the shared code-validation rule.
    // ============================================================================

    #region Details & lifecycle

    public void ChangeGroup(Guid specificationGroupId)
    {
        SpecificationGroupId = specificationGroupId;
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    public void UpdateDefaultUnit(string? defaultUnit)
    {
        DefaultUnit = defaultUnit;
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
            throw ExceptionFactory.RequiredField("Specification definition code cannot be empty.");
    }

    #endregion
}
