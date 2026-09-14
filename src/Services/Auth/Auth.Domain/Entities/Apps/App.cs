using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.Auth.Domain.Entities.Apps;

/// <summary>
/// A client-application boundary a user can be assigned to (e.g. "storefront_web",
/// "admin_portal") - distinct from Tenant (customer/organization boundary). The frontend
/// hardcodes an App's Code before starting the authentication flow, the same way it hardcodes a
/// TenantClient's PublicKey.
/// </summary>
public sealed class App : AggregateRoot<Guid>, IAuditable
{
    public AppCode Code { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public ICollection<AppTranslation> Translations { get; private set; } = [];

    private App() { }

    public static App Create(AppCode code, string name)
    {
        ValidateName(name);

        return new App
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Name = name,
            IsActive = true,
        };
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

        var translation = AppTranslation.Create(
            Id,
            languageCode,
            displayName,
            description);
        Translations.Add(translation);
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Display-name renaming and Active/Inactive status. Code has no change method -
    // it is the stable identifier the frontend and User-App assignments key off of.
    // ============================================================================

    #region Details & lifecycle

    public void Rename(string name)
    {
        ValidateName(name);
        Name = name;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public static bool IsValidName(string? name)
        => name.IsNotNullOrWhiteSpace();

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("App name cannot be empty.");
    }

    #endregion
}
