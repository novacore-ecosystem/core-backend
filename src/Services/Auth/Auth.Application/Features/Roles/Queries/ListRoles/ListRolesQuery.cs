namespace NovaCore.Auth.Application.Features.Roles.Queries.ListRoles;

public sealed record ListRolesQuery : IQuery<IReadOnlyList<RoleSummaryResponse>>;

public sealed record RoleSummaryResponse(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsSystemRole);
