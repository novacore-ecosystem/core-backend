using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Options;

/// <summary>
/// Independent, reusable catalog lookup - a globally shared option dimension (e.g. "Color",
/// "Size") that Products opt into via <see cref="ProductOption"/> instead of redefining. Owns
/// the reusable <see cref="ProductOptionValueDefinition"/> catalog for that dimension.
/// </summary>
public sealed class ProductOptionDefinition : AggregateRoot<Guid>, IAuditable
{
    public string Code { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public CatalogStatus Status { get; private set; } = CatalogStatus.Active;
    public ProductOptionDefinitionMetadata Metadata { get; private set; } = new();

    public ICollection<ProductOptionDefinitionTranslation> Translations { get; private set; } = [];
    public ICollection<ProductOptionValueDefinition> ValueDefinitions { get; private set; } = [];

    private ProductOptionDefinition() { }

    public static ProductOptionDefinition Create(
        Guid id,
        string code,
        int displayOrder = 0,
        CatalogStatus status = CatalogStatus.Active,
        ProductOptionDefinitionMetadata? metadata = null)
    {
        ValidateCode(code);

        return new ProductOptionDefinition
        {
            Id = id,
            Code = code,
            DisplayOrder = displayOrder,
            Status = status,
            Metadata = metadata ?? new ProductOptionDefinitionMetadata(),
        };
    }

    // ============================================================================
    // Value Definitions
    // Owns the reusable ProductOptionValueDefinition catalog for this dimension:
    // creation, removal, and display-order sequencing. ProductOptionValueDefinition
    // is never constructed outside this entity.
    // ============================================================================

    #region Value Definitions

    public ProductOptionValueDefinition AddValue(
        string code,
        Guid? id = null,
        int? displayOrder = null,
        CatalogStatus status = CatalogStatus.Active,
        ProductOptionValueDefinitionMetadata? metadata = null)
    {
        var order = displayOrder ?? (ValueDefinitions.Count == 0 ? 0 : ValueDefinitions.Max(v => v.DisplayOrder) + 1);

        var value = ProductOptionValueDefinition.Create(
            id ?? Guid.CreateVersion7(),
            Id,
            code,
            order,
            status,
            metadata);
        ValueDefinitions.Add(value);

        return value;
    }

    public void RemoveValue(Guid valueDefinitionId)
    {
        var value = ValueDefinitions.FirstOrDefault(v => v.Id == valueDefinitionId);
        if (value is null)
            return;

        ValueDefinitions.Remove(value);
    }

    public void ReorderValues(IEnumerable<Guid> orderedValueDefinitionIds)
    {
        var idsInOrder = orderedValueDefinitionIds.ToArray();

        for (var i = 0; i < idsInOrder.Length; i++)
        {
            var value = ValueDefinitions.FirstOrDefault(v => v.Id == idsInOrder[i])
                ?? throw ExceptionFactory.EntityNotFound<ProductOptionValueDefinition>(idsInOrder[i]);
            value.ChangeDisplayOrder(i);
        }
    }

    #endregion

    // ============================================================================
    // Translations
    // Manages the per-language display name/description override for this
    // option dimension, one entry per language code.
    // ============================================================================

    #region Translations

    public void AddTranslation(LanguageCode languageCode, string name, string? description = null)
    {
        if (Translations.Any(t => t.LanguageCode == languageCode))
            throw ExceptionFactory.Duplicate($"A translation for language '{languageCode}' already exists.");

        Translations.Add(ProductOptionDefinitionTranslation.Create(Id, languageCode, name, description));
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
    // Display ordering, metadata updates, and Active/Inactive status
    // transitions, plus the shared code-validation rule.
    // ============================================================================

    #region Details & lifecycle

    public void ChangeDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    public void UpdateMetadata(ProductOptionDefinitionMetadata metadata)
    {
        Metadata = metadata;
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
            throw ExceptionFactory.RequiredField("Option definition code cannot be empty.");
    }

    #endregion
}
