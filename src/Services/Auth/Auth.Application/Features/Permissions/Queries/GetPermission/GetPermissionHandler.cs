using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;

namespace NovaCore.Auth.Application.Features.Permissions.Queries.GetPermission;

public sealed class GetPermissionHandler(IPermissionReadService permissionReadService)
    : IQueryHandler<GetPermissionQuery, PermissionDetailResponse>
{
    public async Task<PermissionDetailResponse> Handle(GetPermissionQuery request, CancellationToken ct = default)
    {
        var permission = await permissionReadService.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Permission", request.Id);

        return new PermissionDetailResponse(
            permission.Id,
            permission.Key.Value,
            permission.PermissionGroupId,
            permission.PermissionGroup.Code.Value,
            permission.IsSystemPermission);
    }
}
