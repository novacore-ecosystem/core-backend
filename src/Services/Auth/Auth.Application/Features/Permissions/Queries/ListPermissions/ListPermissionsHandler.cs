using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;

namespace NovaCore.Auth.Application.Features.Permissions.Queries.ListPermissions;

public sealed class ListPermissionsHandler(IPermissionReadService permissionReadService)
    : IQueryHandler<ListPermissionsQuery, IReadOnlyList<PermissionSummaryResponse>>
{
    public async Task<IReadOnlyList<PermissionSummaryResponse>> Handle(ListPermissionsQuery request, CancellationToken ct = default)
    {
        var permissions = await permissionReadService.ListAsync(ct);

        return [.. permissions.Select(p => new PermissionSummaryResponse(p.Id, p.Key.Value, p.PermissionGroup.Code.Value, p.IsSystemPermission))];
    }
}
