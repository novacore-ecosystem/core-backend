using NovaCore.Auth.Application.Abstractions.Persistence.Roles;

namespace NovaCore.Auth.Application.Features.Roles.Queries.ListRoles;

public sealed class ListRolesHandler(IRoleReadService roleReadService)
    : IQueryHandler<ListRolesQuery, IReadOnlyList<RoleSummaryResponse>>
{
    public async Task<IReadOnlyList<RoleSummaryResponse>> Handle(ListRolesQuery request, CancellationToken ct = default)
    {
        var roles = await roleReadService.ListAsync(ct);

        return [.. roles.Select(r => new RoleSummaryResponse(r.Id, r.Name!, r.Code.Value, r.Description, r.IsSystemRole))];
    }
}
