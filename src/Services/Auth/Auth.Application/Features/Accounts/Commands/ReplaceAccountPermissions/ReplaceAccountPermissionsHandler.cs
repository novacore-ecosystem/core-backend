using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Contract.Events.User;
using NovaCore.BuildingBlock.SharedKernel.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Context;

namespace NovaCore.Auth.Application.Features.Accounts.Commands.ReplaceAccountPermissions;

/// <summary>Two-phase commit, matching UpdateRolePermissionsHandler's own shape: the direct
/// PermissionGrant mutation commits first (PermissionGrantService self-commits), then this one
/// Account's recomputed effective permissions are enqueued as a second SaveChanges.</summary>
public sealed class ReplaceAccountPermissionsHandler(
    ICurrentUserService currentUserService,
    IAccountAuthorizationGuard authorizationGuard,
    IPermissionGrantService permissionGrantService,
    IEffectivePermissionReadService effectivePermissionReadService,
    IOutboxStore outboxStore,
    IUnitOfWork unitOfWork) : ICommandHandler<ReplaceAccountPermissionsCommand>
{
    public async Task Handle(ReplaceAccountPermissionsCommand request, CancellationToken ct = default)
    {
        var tenantId = RequestContext.Current.TenantId ?? Guid.Empty;
        var actorId = currentUserService.GetUserId() ?? throw new UnauthorizedException();
        var providerKey = request.AccountId.ToString();

        await authorizationGuard.EnsureCanManageAccountAsync(actorId, request.AccountId, tenantId, ct);

        // Only the newly-added keys need the "actor must already hold this permission" check -
        // see ReplaceAccountRolesHandler/IAccountAuthorizationGuard for the same reasoning applied
        // to roles.
        var currentKeys = await permissionGrantService.GetGrantedKeysAsync(PermissionProviderName.User, providerKey, tenantId, ct);
        var newlyAddedKeys = request.PermissionKeys.Where(key => !currentKeys.Contains(key)).ToArray();
        await authorizationGuard.EnsureCanGrantPermissionsAsync(actorId, newlyAddedKeys, tenantId, ct);

        var result = await permissionGrantService.ReplaceForProviderAsync(
            PermissionProviderName.User, providerKey, request.PermissionKeys, tenantId, ct);
        if (!result.HasChanges)
            return;

        var effectivePermissions = await effectivePermissionReadService.GetEffectivePermissionsAsync(request.AccountId, tenantId, ct);

        await outboxStore.EnqueueAsync(
            new AccountEffectivePermissionsChangedIntegrationEvent(
                tenantId,
                [new AccountEffectivePermissions(request.AccountId, [.. effectivePermissions])]),
            ct);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
