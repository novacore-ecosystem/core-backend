using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;
using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.AssignTenantPermissions;

/// <summary>
/// Grants/revokes keys on the Tenant's own PermissionGrant row set (ProviderName = Tenant,
/// ProviderKey = tenantId) - the boundary AccountAuthorizationGuard.EnsureWithinTenantBoundary
/// enforces against that tenant's Role/User grants. Endpoint-gated to Permissions.Tenant.Manage,
/// same as every other Tenant Management operation - this is a ROOT capability, not something a
/// tenant configures for itself.
/// </summary>
public sealed class AssignTenantPermissionsHandler(
    ITenantReadService tenantReadService,
    IPermissionGrantService permissionGrantService) : ICommandHandler<AssignTenantPermissionsCommand>
{
    public async Task Handle(AssignTenantPermissionsCommand request, CancellationToken ct = default)
    {
        _ = await tenantReadService.GetByIdAsync(request.TenantId, ct)
            ?? throw new NotFoundException("Tenant", request.TenantId);

        var providerKey = request.TenantId.ToString();

        foreach (var key in request.Revoke)
            await permissionGrantService.RevokeAsync(key, PermissionProviderName.Tenant, providerKey, request.TenantId, ct);

        foreach (var key in request.Grant)
            await permissionGrantService.GrantAsync(key, PermissionProviderName.Tenant, providerKey, request.TenantId, ct);
    }
}
