namespace NovaCore.User.Domain.Entities.Roles;

/// <summary>
/// Owned child of UserRole - a locale-specific override of the role's display text. UserRole.Key
/// is the internal, language-independent identifier and must remain unchanged; DisplayName here
/// is what's shown on client applications and admin portals.
/// </summary>
public sealed class UserRoleTranslation : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public UserRole Role { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string DisplayName { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private UserRoleTranslation() { }

    public static UserRoleTranslation Create(
        Guid roleId,
        LanguageCode languageCode,
        string displayName,
        string? description = null)
    {
        ValidateDisplayName(displayName);

        return new UserRoleTranslation
        {
            Id = roleId,
            LanguageCode = languageCode,
            DisplayName = displayName,
            Description = description,
        };
    }

    public void UpdateDetails(string displayName, string? description)
    {
        ValidateDisplayName(displayName);

        DisplayName = displayName;
        Description = description;
    }

    public static bool IsValidDisplayName(string? displayName) => !string.IsNullOrWhiteSpace(displayName);

    private static void ValidateDisplayName(string displayName)
    {
        if (!IsValidDisplayName(displayName))
            throw ExceptionFactory.RequiredField("Role display name cannot be empty.");
    }
}
