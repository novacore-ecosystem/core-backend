namespace NovaCore.Auth.Application.Features.Permissions.Queries.ListPermissions;

public sealed record ListPermissionsQuery : IQuery<IReadOnlyList<PermissionSummaryResponse>>;

public sealed record PermissionSummaryResponse(
    Guid Id,
    string Key,
    string PermissionGroupCode,
    bool IsSystemPermission);
