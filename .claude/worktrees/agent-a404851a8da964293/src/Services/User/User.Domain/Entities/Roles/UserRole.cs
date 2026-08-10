namespace NovaCore.User.Domain.Entities.Roles;

/// <summary>
/// Independent, reusable permission bundle assigned to users - a flexible, permission-based
/// authorization unit ("admin", "warehouse_manager", "customer_support"). Users never hold a
/// Role reference directly; they hold UserRoleAssignment rows pointing at a RoleId. Admin-managed,
/// so it supports localized display text via Translations - Key itself is the internal,
/// language-independent identifier and is never translated.
/// </summary>
public sealed class UserRole : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public RoleKey Key { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public RoleStatus Status { get; private set; } = RoleStatus.Active;
    public PermissionCollection Permissions { get; private set; } = PermissionCollection.Empty;
    public ICollection<UserRoleTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private UserRole() { }

    public static UserRole Create(
        Guid id,
        RoleKey key,
        string description,
        RoleStatus status = RoleStatus.Active,
        PermissionCollection? permissions = null)
    {
        return new UserRole
        {
            Id = id,
            Key = key,
            Description = description,
            Status = status,
            Permissions = permissions ?? PermissionCollection.Empty,
        };
    }

    // ============================================================================
    // Permissions
    // Manages the PermissionCollection Value Object - every grant/revoke replaces
    // it wholesale (VO immutability) rather than mutating in place.
    // ============================================================================

    #region Permissions

    public void GrantPermission(string permission)
    {
        Permissions = Permissions.Add(permission);
    }

    public void RevokePermission(string permission)
    {
        Permissions = Permissions.Remove(permission);
    }

    public bool HasPermission(string permission) => Permissions.Contains(permission);

    #endregion

    // ============================================================================
    // Translations
    // Manages the per-language DisplayName/Description override for this role,
    // upserting by language code. The internal Key is never part of a
    // translation.
    // ============================================================================

    #region Translations

    public void Translate(LanguageCode languageCode, string displayName, string? description = null)
    {
        var existingTranslation = Translations
            .FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existingTranslation != null)
        {
            existingTranslation.UpdateDetails(displayName, description);
            return;
        }

        var translation = UserRoleTranslation.Create(
            Id,
            languageCode,
            displayName,
            description);
        Translations.Add(translation);
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Description updates and the Active/Inactive status transition. Key has no
    // change method - it is the stable identifier other services key off of and
    // must remain unchanged for the role's lifetime.
    // ============================================================================

    #region Details & lifecycle

    public void UpdateDescription(string description)
    {
        Description = description;
    }

    public void Activate()
    {
        Status = RoleStatus.Active;
    }

    public void Deactivate()
    {
        Status = RoleStatus.Inactive;
    }

    #endregion
}
