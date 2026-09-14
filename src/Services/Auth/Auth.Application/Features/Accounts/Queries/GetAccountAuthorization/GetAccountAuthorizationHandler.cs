using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;
using NovaCore.BuildingBlock.SharedKernel.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Context;

namespace NovaCore.Auth.Application.Features.Accounts.Queries.GetAccountAuthorization;

public sealed class GetAccountAuthorizationHandler(
    IAccountReadService accountReadService,
    IPermissionGrantService permissionGrantService) : IQueryHandler<GetAccountAuthorizationQuery, AccountAuthorizationResponse>
{
    public async Task<AccountAuthorizationResponse> Handle(GetAccountAuthorizationQuery request, CancellationToken ct = default)
    {
        var tenantId = RequestContext.Current.TenantId ?? Guid.Empty;

        var account = await accountReadService.GetByIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException("Account", request.AccountId);

        var roleIds = await accountReadService.GetRoleIdsAsync(request.AccountId, ct);
        var permissionKeys = await permissionGrantService.GetGrantedKeysAsync(
            PermissionProviderName.User, request.AccountId.ToString(), tenantId, ct);

        return new AccountAuthorizationResponse(account.Id, account.Level, [.. roleIds], [.. permissionKeys]);
    }
}
