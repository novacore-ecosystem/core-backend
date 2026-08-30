using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Auth.Domain.Entities.Permissions;

public sealed class PermissionDefinition : AggregateRoot<Guid>, IAuditable
{
    public PermissionKey Key { get; private set; } = null!;
    public Guid PermissionGroupId { get; private set; }
    public PermissionGroup PermissionGroup { get; private set; } = default!;
    public bool IsSystemPermission { get; private set; }
    public PermissionDefinitionStatus Status { get; private set; } = PermissionDefinitionStatus.Active;

    public ICollection<PermissionGrant> Grants { get; private set; } = [];
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
    // Regrouping. Key has no change method - it is the stable identifier baked
    // into JWT claims and policy checks. IsSystemPermission only blocks deletion
    // (see Application-layer DeletePermissionHandler) - a system permission can
    // still be regrouped/updated like any other, per the "can update, cannot
    // delete" invariant documented on Permissions.Root/Permissions.User.
    // ============================================================================

    #region Details

    public void MoveToGroup(Guid permissionGroupId)
    {
        PermissionGroupId = permissionGroupId;
    }

    #endregion

    // ============================================================================
    // Lifecycle
    // Definition status only - a maintenance/discoverability signal (e.g. hide from an
    // admin picker, flag as scheduled for removal). Deliberately does not touch or cascade
    // to any PermissionGrant referencing this key - grant state is a separate concern.
    // ============================================================================

    #region Lifecycle

    public void Activate()
    {
        Status = PermissionDefinitionStatus.Active;
    }

    public void Deprecate()
    {
        Status = PermissionDefinitionStatus.Deprecated;
    }

    public void Disable()
    {
        Status = PermissionDefinitionStatus.Disabled;
    }

    #endregion
}
