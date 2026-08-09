namespace NovaCore.Auth.Application.Features.Permissions.Queries.GetPermission;

public sealed record GetPermissionQuery(Guid Id) : IQuery<PermissionDetailResponse>;

public sealed record PermissionDetailResponse(
    Guid Id,
    string Key,
    Guid PermissionGroupId,
    string PermissionGroupCode,
    bool IsSystemPermission);
