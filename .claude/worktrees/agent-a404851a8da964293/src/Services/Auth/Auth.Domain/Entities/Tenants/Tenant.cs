using NovaCore.Auth.Domain.Metadata;
using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.Auth.Domain.Entities.Tenants;

/// <summary>
/// One customer/company using the platform - the tenant-foundation root every future
/// tenant-scoped aggregate (Scope, and eventually every business aggregate) hangs off of via
/// TenantId. Carries only bootstrap/branding identity - subscription, billing, licensing and
/// feature management are explicitly out of scope for this aggregate.
/// </summary>
public sealed class Tenant : AggregateRoot<Guid>, IAuditable
{
    public TenantCode Code { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string? LogoUrl { get; private set; }
    public string? FaviconUrl { get; private set; }
    public int Version { get; private set; } = 1;
    public TenantMetadata Metadata { get; private set; } = new();
    public bool IsActive { get; private set; } = true;

    public ICollection<TenantLocale> Locales { get; private set; } = [];

    private Tenant() { }

    public static Tenant Create(
        TenantCode code,
        string name,
        string? logoUrl = null,
        string? faviconUrl = null,
        TenantMetadata? metadata = null)
    {
        ValidateName(name);

        var tenant = new Tenant
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Name = name,
            LogoUrl = logoUrl,
            FaviconUrl = faviconUrl,
            Version = 1,
            Metadata = metadata ?? new TenantMetadata(),
            IsActive = true,
        };

        return tenant;
    }

    // ============================================================================
    // Locales
    // Manages the owned TenantLocale collection - the entire bootstrap resource set
    // (config + dictionary) for one locale, upserting by language code. A null language
    // code identifies the fallback resource (see TenantLocale).
    // ============================================================================

    #region Locales

    public void SetLocale(
        LanguageCode? languageCode,
        string configurationJson,
        string dictionaryJson)
    {
        var existingLocale = Locales.FirstOrDefault(l => l.LanguageCode == languageCode);
        if (existingLocale != null)
        {
            existingLocale.UpdateContent(configurationJson, dictionaryJson);
            return;
        }

        var locale = TenantLocale.Create(
            Id,
            languageCode,
            configurationJson,
            dictionaryJson);
        Locales.Add(locale);
    }

    public void RemoveLocale(LanguageCode? languageCode)
    {
        var locale = Locales.FirstOrDefault(l => l.LanguageCode == languageCode);
        if (locale is null)
            return;

        Locales.Remove(locale);
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Branding/name updates, bootstrap version bumping, and Active/Inactive status.
    // Code has no change method - it is the stable identifier tenant-scoped data keys off of.
    // ============================================================================

    #region Details & lifecycle

    public void Rename(string name)
    {
        ValidateName(name);
        Name = name;
    }

    public void UpdateBranding(string? logoUrl, string? faviconUrl)
    {
        LogoUrl = logoUrl;
        FaviconUrl = faviconUrl;
    }

    public void UpdateMetadata(TenantMetadata metadata)
    {
        Metadata = metadata;
    }

    /// <summary>Bumps the bootstrap version - callers signal this whenever bootstrap-relevant
    /// data (locales, branding, ...) changes in a way clients must reload for.</summary>
    public void IncrementVersion()
    {
        Version++;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public static bool IsValidName(string? name) => name.IsNotNullOrWhiteSpace();

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Tenant name cannot be empty.");
    }

    #endregion
}
