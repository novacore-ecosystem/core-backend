using NovaCore.Auth.Application.Common;

namespace NovaCore.Auth.Application.Features.Tenants.Queries.GetTenantPermissions;

/// <summary>The tenant's permission boundary - the catalog subset ROOT has allowed this tenant's
/// own Role/User grants to draw from, plus whether that boundary is currently enforced. See
/// AccountAuthorizationGuard.EnsureWithinTenantBoundary.</summary>
public sealed record GetTenantPermissionsQuery(Guid TenantId) : IQuery<TenantPermissionsResponse>;

public sealed record TenantPermissionsResponse(
    IReadOnlyList<string> PermissionKeys,
    bool BoundaryEnabled);
