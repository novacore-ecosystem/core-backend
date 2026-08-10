using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.Auth.Domain.Entities.Permissions;

/// <summary>
/// Owned child of PermissionGroup - a locale-specific override of the group's admin-facing
/// display copy. Id doubles as the owning PermissionGroup's Id (one row per language, see
/// PermissionGroupTranslationConfig for the composite (Id, LanguageCode) key).
/// </summary>
public sealed class PermissionGroupTranslation : BaseEntity<Guid>, IAuditable
{
    public PermissionGroup PermissionGroup { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string DisplayName { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private PermissionGroupTranslation() { }

    public static PermissionGroupTranslation Create(
        Guid permissionGroupId,
        LanguageCode languageCode,
        string displayName,
        string? description = null)
    {
        ValidateDisplayName(displayName);

        return new PermissionGroupTranslation
        {
            Id = permissionGroupId,
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
