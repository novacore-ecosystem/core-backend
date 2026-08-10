using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.Auth.Domain.Entities.Permissions;

/// <summary>
/// Owned child of PermissionDefinition - a locale-specific override of the permission's
/// admin-facing display copy. Id doubles as the owning PermissionDefinition's Id (one row per
/// language, see PermissionDefinitionTranslationConfig for the composite (Id, LanguageCode) key).
/// </summary>
public sealed class PermissionDefinitionTranslation : BaseEntity<Guid>, IAuditable
{
    public PermissionDefinition PermissionDefinition { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string DisplayName { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private PermissionDefinitionTranslation() { }

    public static PermissionDefinitionTranslation Create(
        Guid permissionDefinitionId,
        LanguageCode languageCode,
        string displayName,
        string? description = null)
    {
        ValidateDisplayName(displayName);

        return new PermissionDefinitionTranslation
        {
            Id = permissionDefinitionId,
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
