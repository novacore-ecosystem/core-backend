using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Auth.Domain.Entities.Apps;

/// <summary>
/// Per-language DisplayName/Description override for an App. Mirrors RoleTranslation's shape:
/// Id doubles as the owning App's Id, so the primary key is the composite (Id, LanguageCode).
/// App is a global (not tenant-scoped) client-application boundary - the frontend resolves it
/// before any tenant claim exists, the same way TenantClient's PublicKey does - so, unlike
/// RoleTranslation, this does not implement ITenantEntity.
/// </summary>
public sealed class AppTranslation : BaseEntity<Guid>, IAuditable
{
    public LanguageCode LanguageCode { get; private set; } = null!;
    public string DisplayName { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public App? App { get; set; }

    private AppTranslation() { }

    public static AppTranslation Create(
        Guid appId,
        LanguageCode languageCode,
        string displayName,
        string? description = null)
    {
        return new AppTranslation
        {
            Id = appId,
            LanguageCode = languageCode,
            DisplayName = displayName,
            Description = description,
        };
    }

    public void UpdateContent(string displayName, string? description)
    {
        DisplayName = displayName;
        Description = description;
    }
}
