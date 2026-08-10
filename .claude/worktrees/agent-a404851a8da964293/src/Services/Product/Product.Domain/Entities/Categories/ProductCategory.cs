using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Product.Domain.Entities.Categories;

public sealed class ProductCategory : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public CategoryCode Code { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ProductCategoryStatus Status { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public string Note { get; private set; } = string.Empty;

    public IEnumerable<ProductCategoryTranslation> Translation { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ProductCategory() { }

    public static ProductCategory Create(
        Guid id,
        CategoryCode code,
        string name,
        string description,
        Guid? parentCategoryId = null,
        ProductCategoryStatus status = ProductCategoryStatus.Active,
        string note = "")
    {
        ValidateName(name);

        if (parentCategoryId == id)
            throw ExceptionFactory.InvalidState("A category cannot be its own parent.");

        return new ProductCategory
        {
            Id = id,
            Code = code,
            Name = name,
            Description = description,
            ParentCategoryId = parentCategoryId,
            Status = status,
            Note = note,
        };
    }

    // ============================================================================
    // Translations
    // Manages the per-language name/description/note override for this
    // category, upserting by language code.
    // ============================================================================

    #region Translations

    public void Translate(
        LanguageCode languageCode,
        string name,
        string description)
    {
        ValidateName(name);

        var existingTranslation = Translation
            .FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existingTranslation != null)
        {
            existingTranslation.UpdateDetails(name, description);
            return;
        }

        var translation = ProductCategoryTranslation.Create(
            Id,
            languageCode,
            name,
            description);

        Translation = Translation.Append(translation);
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Core descriptive fields, parent-category reassignment, and Active/Inactive
    // status transitions, plus the shared name-validation rule.
    // ============================================================================

    #region Details & lifecycle

    public void UpdateDetails(string name, string description, string note)
    {
        ValidateName(name);

        Name = name;
        Description = description;
        Note = note;
    }

    /// <summary>
    /// Moves this category under a new parent (or to root when null). Only guards against
    /// direct self-parenting - detecting a deeper ancestor cycle (this category being moved
    /// under one of its own descendants) requires querying the full category tree, which is
    /// an Application-layer concern (repository lookup across many ProductCategory instances),
    /// not something a single aggregate instance can verify on its own.
    /// </summary>
    public void ChangeParent(Guid? parentCategoryId)
    {
        if (parentCategoryId == Id)
            throw ExceptionFactory.InvalidState("A category cannot be its own parent.");

        ParentCategoryId = parentCategoryId;
    }

    public void Activate()
    {
        Status = ProductCategoryStatus.Active;
    }

    public void Deactivate()
    {
        Status = ProductCategoryStatus.Inactive;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Category name cannot be empty.");
    }

    #endregion
}
