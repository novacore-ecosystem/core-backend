namespace NovaCore.Auth.Application.Features.Roles.Queries.GetRole;

public sealed record GetRoleQuery(Guid Id) : IQuery<RoleDetailResponse>;

public sealed record RoleDetailResponse(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsSystemRole,
    IReadOnlyList<string> Permissions);
