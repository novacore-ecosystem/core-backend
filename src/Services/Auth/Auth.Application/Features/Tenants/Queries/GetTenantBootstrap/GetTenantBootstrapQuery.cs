using NovaCore.Auth.Application.Common;

namespace NovaCore.Auth.Application.Features.Tenants.Queries.GetTenantBootstrap;

/// <summary>Pre-authentication tenant bootstrap - identified by TenantClient.PublicKey, the same
/// mechanism Login uses to resolve a Tenant before credentials are checked (see LoginCommand).
/// Deliberately lightweight - only what the frontend needs to render its initial shell before a
/// user logs in (branding, effective translations, the bootstrap Version). Never the full Tenant
/// Management editing payload - see GetTenantQuery for that.</summary>
public sealed record GetTenantBootstrapQuery(string ClientPublicKey) : IQuery<TenantBootstrapResponse>;

public sealed record TenantBootstrapResponse(
    int Version,
    TenantBootstrapInfo Tenant,
    IReadOnlyList<string> SupportedLanguages,
    IReadOnlyDictionary<string, EffectiveTranslationResponse> Translations);

public sealed record TenantBootstrapInfo(
    Guid Id,
    string Code,
    string Name,
    string? LogoUrl,
    string? FaviconUrl);
