namespace NovaCore.Auth.Application.Features.Accounts.Queries.GetAccountAuthorization;

public sealed record GetAccountAuthorizationQuery(Guid AccountId) : IQuery<AccountAuthorizationResponse>;

public sealed record AccountAuthorizationResponse(
    Guid AccountId,
    int Level,
    IReadOnlyList<Guid> RoleIds,
    IReadOnlyList<string> PermissionKeys);
