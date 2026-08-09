using NovaCore.Auth.Application.Abstractions.Persistence.Roles;

namespace NovaCore.Auth.Application.Features.Roles.Queries.GetRole;

public sealed class GetRoleHandler(IRoleReadService roleReadService) : IQueryHandler<GetRoleQuery, RoleDetailResponse>
{
    public async Task<RoleDetailResponse> Handle(GetRoleQuery request, CancellationToken ct = default)
    {
        var role = await roleReadService.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Role", request.Id);

        return new RoleDetailResponse(
            role.Id,
            role.Name!,
            role.Code.Value,
            role.Description,
            role.IsSystemRole,
            [.. role.Permissions.Select(p => p.PermissionDefinition.Key.Value)]);
    }
}
