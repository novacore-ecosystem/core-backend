using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Auth.Domain.Entities.Permissions;

/// <summary>
/// Admin-UI grouping for PermissionDefinitions (e.g. "Product Management", "Order Management").
/// Purely organizational - carries no authorization weight itself.
/// </summary>
public sealed class PermissionGroup : AggregateRoot<Guid>, IAuditable
{
    public PermissionGroupCode Code { get; private set; } = null!;
    public int SortOrder { get; private set; }

    public ICollection<PermissionDefinition> Definitions { get; private set; } = [];
    public ICollection<PermissionGroupTranslation> Translations { get; private set; } = [];

    private PermissionGroup() { }

    public static PermissionGroup Create(PermissionGroupCode code, int sortOrder = 0)
    {
        return new PermissionGroup
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            SortOrder = sortOrder,
        };
    }

    // ============================================================================
    // Translations
    // Manages per-language DisplayName/Description overrides, upserting by
    // language code. Code itself is never translated.
    // ============================================================================

    #region Translations

    public void Translate(
        LanguageCode languageCode,
        string displayName,
        string? description = null)
    {
        var existingTranslation = Translations
            .FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existingTranslation != null)
        {
            existingTranslation.UpdateContent(displayName, description);
            return;
        }

        var translation = PermissionGroupTranslation.Create(
            Id,
            languageCode,
            displayName,
            description);
        Translations.Add(translation);
    }

    #endregion

    // ============================================================================
    // Details
    // Display ordering for the admin UI.
    // ============================================================================

    #region Details

    public void Reorder(int sortOrder)
    {
        SortOrder = sortOrder;
    }

    #endregion
}
