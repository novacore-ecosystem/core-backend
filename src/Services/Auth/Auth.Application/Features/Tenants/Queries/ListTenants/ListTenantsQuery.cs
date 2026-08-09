namespace NovaCore.Auth.Application.Features.Tenants.Queries.ListTenants;

/// <summary>Root Portal tenant discovery/selection only (see docs/services/auth-service.md,
/// Phase 2) - deliberately excludes Metadata/Version/FaviconUrl (bootstrap-only concerns) and
/// anything from TenantClient (PublicKey must never appear in a response body, see TenantClient's
/// class doc comment).</summary>
public sealed record ListTenantsQuery : IQuery<IReadOnlyList<TenantSummaryResponse>>;

public sealed record TenantSummaryResponse(
    Guid Id,
    string Code,
    string Name,
    string? LogoUrl,
    bool IsActive);
