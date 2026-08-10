using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.Auth.Domain.Entities.Roles;

/// <summary>
/// Owned child of Role - a locale-specific override of the role's admin-facing display copy.
/// Id doubles as the owning Role's Id (one row per language, see RoleTranslationConfig for the
/// composite (Id, LanguageCode) key).
/// </summary>
public sealed class RoleTranslation : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Role Role { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string DisplayName { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private RoleTranslation() { }

    public static RoleTranslation Create(
        Guid roleId,
        LanguageCode languageCode,
        string displayName,
        string? description = null)
    {
        ValidateDisplayName(displayName);

        return new RoleTranslation
        {
            Id = roleId,
            LanguageCode = languageCode,
            DisplayName = displayName,
            Description = description,
        };
    }

    public void UpdateContent(string displayName, string? description)
    {
        ValidateDisplayName(displayName);

        DisplayName = displayName;
        Description = description;
    }

    public static bool IsValidDisplayName(string? displayName) => displayName.IsNotNullOrWhiteSpace();

    private static void ValidateDisplayName(string displayName)
    {
        if (!IsValidDisplayName(displayName))
            throw ExceptionFactory.RequiredField("Translated display name cannot be empty.");
    }
}
