using Microsoft.AspNetCore.Identity;

using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.BuildingBlock.Domain.Attributes;
using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.Auth.Domain.Entities.Roles;

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
