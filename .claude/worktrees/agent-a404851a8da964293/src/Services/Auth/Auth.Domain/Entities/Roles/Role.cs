using Microsoft.AspNetCore.Identity;

using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.BuildingBlock.Domain.Attributes;
using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Auth.Domain.Entities.Roles;

/// <summary>
/// Named, reusable permission bundle - independent of User.Domain's UserRole (business-domain
/// role management), this Role exists purely to answer "what does this token holder's Auth
/// service think they can do." Normally referenced indirectly, many-to-many, through Position
/// (PositionRole) - the same Role (e.g. "Inventory Operator") is shared across every Position
/// that needs it, so permissions are never duplicated per Position. AccountRole (direct
/// Account-to-Role assignment) remains as the exceptional path for grants that don't map to a
/// Position - see Account's class doc comment.
/// </summary>
public sealed class Role : IdentityRole<Guid>, IEntity, IAuditable
{
    public RoleCode Code { get; private set; } = null!;
    public string? Description { get; set; }
    public bool IsSystemRole { get; private set; }

    public ICollection<AccountRole> UserRoles { get; private set; } = [];
    public ICollection<RolePermission> Permissions { get; private set; } = [];
    public ICollection<RoleTranslation> Translations { get; private set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [AuditIgnore]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static Role Create(
        string name,
        RoleCode code,
        string? description = null,
        bool isSystemRole = false)
    {
        return new Role
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            Code = code,
            Description = description,
            IsSystemRole = isSystemRole,
        };
    }

    public void Track()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    // ============================================================================
    // Permission
    // Manages the RolePermission join collection - which PermissionDefinitions this
    // Role grants. Assignment changes here must be followed by an
    // Account.RefreshPermissionSnapshot() call for affected accounts.
    // ============================================================================

    #region Permission

    public void AssignPermission(PermissionDefinition permission)
    {
        if (Permissions.Any(p => p.PermissionDefinitionId == permission.Id))
            return;

        var rolePermission = RolePermission.Create(Id, permission.Id);
        Permissions.Add(rolePermission);
    }

    public void RemovePermission(Guid permissionDefinitionId)
    {
        var rolePermission = Permissions
            .FirstOrDefault(p => p.PermissionDefinitionId == permissionDefinitionId);
        if (rolePermission is null)
            return;

        Permissions.Remove(rolePermission);
    }

    #endregion

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

        var translation = RoleTranslation.Create(
            Id,
            languageCode,
            displayName,
            description);
        Translations.Add(translation);
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Display-name renaming and system-role protection. Code has no change method -
    // it is the stable identifier assignments key off of.
    // ============================================================================

    #region Details & lifecycle

    public void Rename(string name)
    {
        if (IsSystemRole)
            throw ExceptionFactory.InvalidState("Cannot rename a system role.");

        Name = name;
        NormalizedName = name.ToUpperInvariant();
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }

    #endregion
}
