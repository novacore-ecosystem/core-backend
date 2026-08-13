using NovaCore.BuildingBlock.Application.Abstractions.Common;

namespace NovaCore.Auth.Application.Features.Tenants.Queries.ListTenants;

/// <summary>Root Portal tenant list/search (see docs/services/auth-service.md, Phase 2) -
/// deliberately excludes Metadata/Version/FaviconUrl (bootstrap-only concerns) and anything from
/// TenantClient (PublicKey must never appear in a response body, see TenantClient's class doc
/// comment). Kept lightweight on purpose - see GetTenantQuery for the full editing DTO.</summary>
public sealed record ListTenantsQuery(
    string? Search = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedResult<TenantSummaryResponse>>;

public sealed record TenantSummaryResponse(
    Guid Id,
    string Code,
    string Name,
    string? LogoUrl,
    bool IsActive);
