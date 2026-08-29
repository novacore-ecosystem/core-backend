using Microsoft.AspNetCore.Identity;

using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.BuildingBlock.Domain.Attributes;
using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.Auth.Domain.Entities.Roles;

/// <summary>
/// Named, reusable permission bundle - independent of User.Domain's UserRole (business-domain
/// role management), this Role exists purely to answer "what does this token holder's Auth
/// service think they can do." Normally referenced indirectly, many-to-many, through Position
/// (PositionRole) - the same Role (e.g. "Inventory Operator") is shared across every Position
/// that needs it, so permissions are never duplicated per Position. AccountRole (direct
/// Account-to-Role assignment) remains as the exceptional path for grants that don't map to a
/// Position - see Account's class doc comment.
///
/// ProviderName classifies which principal-category catalog this Role belongs to (e.g. every Role
/// assignable to an Account is ProviderName == User) - it is NOT a per-instance owner, Role stays
/// a single global, reusable catalog shared across every Position/Account that references it (see
/// "Global vs tenant-scoped" in docs/services/auth-service.md). This is what lets a future Client/
/// Guest principal reuse the same Role table via its own ProviderName/join, instead of a new
/// ClientRole/GuestRole table. ProviderKey is a reserved narrower-scoping hook (e.g. a role private
/// to one Client's own catalog) - unused/null for every Role seeded today. Permission grants for a
/// Role are held in the generic PermissionGrant table (ProviderName = Role, ProviderKey = this
/// Role's Id) - Role deliberately does not own a permission-grant collection itself, since
/// PermissionGrant must not carry a real FK back to any one specific provider type.
/// </summary>
public sealed class Role : IdentityRole<Guid>, IEntity, IAuditable
{
    public RoleCode Code { get; private set; } = null!;
    public string? Description { get; set; }
    public bool IsSystemRole { get; private set; }
    public PermissionProviderName ProviderName { get; private set; }
    public string? ProviderKey { get; private set; }

    public ICollection<AccountRole> UserRoles { get; private set; } = [];
    public ICollection<RoleTranslation> Translations { get; private set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [AuditIgnore]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static Role Create(
        string name,
        RoleCode code,
        string? description = null,
        bool isSystemRole = false,
        PermissionProviderName providerName = PermissionProviderName.User,
        string? providerKey = null)
    {
        // "Role" is a valid PermissionGrant.ProviderName (a grant belonging to a Role) but not a
        // valid Role.ProviderName - a Role is a principal-category catalog entry, not itself a
        // principal category.
        if (!providerName.IsSingleValue() || providerName == PermissionProviderName.Role)
            throw ExceptionFactory.InvalidRange(
                $"Role.ProviderName must be exactly one non-Role provider category, got \"{providerName}\".");

        return new Role
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            Code = code,
            Description = description,
            IsSystemRole = isSystemRole,
            ProviderName = providerName,
            ProviderKey = providerKey,
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
