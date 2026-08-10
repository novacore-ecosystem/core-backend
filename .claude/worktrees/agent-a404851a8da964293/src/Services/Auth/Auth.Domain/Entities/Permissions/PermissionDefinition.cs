using NovaCore.Auth.Domain.Entities.Roles;
using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Auth.Domain.Entities.Permissions;

/// <summary>
/// Catalog entry for a single permission key the platform recognizes (e.g. "product:create").
/// Roles grant these via RolePermission; the Key is the literal string that ends up in JWT claims
/// and is checked by every service's [Authorize(Policy = ...)].
/// </summary>
public sealed class PermissionDefinition : AggregateRoot<Guid>, IAuditable
{
    public PermissionKey Key { get; private set; } = null!;
    public Guid PermissionGroupId { get; private set; }
    public PermissionGroup PermissionGroup { get; private set; } = default!;
    public bool IsSystemPermission { get; private set; }

    public ICollection<RolePermission> RolePermissions { get; private set; } = [];
    public ICollection<PermissionDefinitionTranslation> Translations { get; private set; } = [];

    private PermissionDefinition() { }

    public static PermissionDefinition Create(
        PermissionKey key,
        Guid permissionGroupId,
        bool isSystemPermission = false)
    {
        return new PermissionDefinition
        {
            Id = Guid.CreateVersion7(),
            Key = key,
            PermissionGroupId = permissionGroupId,
            IsSystemPermission = isSystemPermission,
        };
    }

    // ============================================================================
    // Translations
    // Manages per-language DisplayName/Description overrides, upserting by
    // language code. Key itself is never translated.
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

        var translation = PermissionDefinitionTranslation.Create(
            Id,
            languageCode,
            displayName,
            description);
        Translations.Add(translation);
    }

    #endregion

    // ============================================================================
    // Details
    // Regrouping and system-permission protection. Key has no change method -
    // it is the stable identifier baked into JWT claims and policy checks.
    // ============================================================================

    #region Details

    public void MoveToGroup(Guid permissionGroupId)
    {
        if (IsSystemPermission)
            throw ExceptionFactory.InvalidState("Cannot regroup a system permission.");

        PermissionGroupId = permissionGroupId;
    }

    #endregion
}
