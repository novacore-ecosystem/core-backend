using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;
using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.Auth.Application.Features.Tenants.Queries.GetTenantPermissions;

public sealed class GetTenantPermissionsHandler(
    ITenantReadService tenantReadService,
    IPermissionGrantService permissionGrantService)
    : IQueryHandler<GetTenantPermissionsQuery, TenantPermissionsResponse>
{
    public async Task<TenantPermissionsResponse> Handle(GetTenantPermissionsQuery request, CancellationToken ct = default)
    {
        var tenant = await tenantReadService.GetByIdAsync(request.TenantId, ct)
            ?? throw new NotFoundException("Tenant", request.TenantId);

        var grantedKeys = await permissionGrantService.GetGrantedKeysAsync(
            PermissionProviderName.Tenant, request.TenantId.ToString(), request.TenantId, ct);

        return new TenantPermissionsResponse([.. grantedKeys], tenant.Metadata.PermissionBoundaryEnabled);
    }
}
