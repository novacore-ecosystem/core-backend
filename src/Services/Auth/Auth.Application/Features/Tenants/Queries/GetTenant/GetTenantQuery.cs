using System.Text.Json;

namespace NovaCore.Auth.Application.Features.Tenants.Queries.GetTenant;

/// <summary>Root Portal Tenant Management editing screen - the comprehensive counterpart to
/// ListTenantsQuery's lightweight summary. Never used for listing (see docs/services/auth-service.md,
/// Tenant Management API DTO Separation).</summary>
public sealed record GetTenantQuery(Guid Id) : IQuery<TenantDetailResponse>;

public sealed record TenantDetailResponse(
    Guid Id,
    string Code,
    string Name,
    string? LogoUrl,
    string? FaviconUrl,
    bool IsActive,
    int Version,
    IReadOnlyList<TenantLocaleResponse> Locales,
    IReadOnlyDictionary<string, EffectiveTranslationResponse> Translations,
    IReadOnlyList<string> SupportedLanguages,
    IReadOnlyList<TenantClientSummaryResponse> Clients,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>Raw, unmerged per-locale content - what the editing UI reads from/writes back to.
/// A null LanguageCode is the fallback/default resource (see TenantLocale).</summary>
public sealed record TenantLocaleResponse(
    string? LanguageCode,
    JsonElement Configuration,
    JsonElement Dictionary);

/// <summary>Effective (fallback merged with this language's override) view for one supported
/// non-default language - see docs/services/auth-service.md, "Merged Tenant Translations". The
/// fallback resource itself is never exposed as an entry here - see GetTenantHandler.</summary>
public sealed record EffectiveTranslationResponse(
    JsonElement Configuration,
    JsonElement Dictionary);

/// <summary>PublicKey is safe to include - it is not a secret (see TenantClient's class doc
/// comment); there is no secret field on TenantClient to redact.</summary>
public sealed record TenantClientSummaryResponse(
    Guid Id,
    string Name,
    string PublicKey,
    string Status,
    DateTime? ExpiresAt,
    DateTime? RevokedAt);
