namespace NovaCore.Auth.Application.Features.Accounts.Queries.GetAccountAuthorization;

public sealed record GetAccountAuthorizationQuery(Guid AccountId) : IQuery<AccountAuthorizationResponse>;

/// <summary>PermissionKeys are this account's direct grants only (editable via
/// ReplaceAccountPermissionsCommand) - not its effective, Role/Position-aggregated set.</summary>
public sealed record AccountAuthorizationResponse(
    Guid AccountId,
    int Level,
    IReadOnlyList<Guid> RoleIds,
    IReadOnlyList<string> PermissionKeys);
