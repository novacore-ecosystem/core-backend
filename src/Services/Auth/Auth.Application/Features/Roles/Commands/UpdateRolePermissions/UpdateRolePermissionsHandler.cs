using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Roles;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Contract.Events.User;
using NovaCore.BuildingBlock.SharedKernel.Context;

namespace NovaCore.Auth.Application.Features.Roles.Commands.UpdateRolePermissions;

/// <summary>
/// Two-phase commit, not one atomic transaction: the Role/RolePermission mutation commits first
/// (RoleWriteService self-commits, matching every other Write Service in Auth), then the affected
/// Accounts' recomputed effective permissions are enqueued as a second SaveChanges. The usual
/// Outbox atomicity guarantee (event + aggregate change in one transaction) is deliberately traded
/// away here - correctly reflecting the JUST-changed RolePermission state requires querying it
/// after it's persisted, and restructuring every Role Write Service call site to defer committing
/// would ripple well beyond this one handler. Worst case on a mid-way failure is a stale
/// UserService projection until the next authorization change, not a correctness bug - see
/// docs/services/auth-service.md, Phase 3.
/// </summary>
public sealed class UpdateRolePermissionsHandler(
    IRoleWriteService roleWriteService,
    IEffectivePermissionReadService effectivePermissionReadService,
    IOutboxStore outboxStore,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateRolePermissionsCommand>
{
    public async Task Handle(UpdateRolePermissionsCommand request, CancellationToken ct = default)
    {
        var tenantId = RequestContext.Current.TenantId ?? Guid.Empty;

        // Role membership (who holds this Role) is unaffected by a permission-set change, so this
        // is safe to resolve before the mutation below.
        var affectedAccountIds = await effectivePermissionReadService.GetAccountIdsForRoleAsync(request.RoleId, tenantId, ct);

        var result = await roleWriteService.UpdatePermissionsAsync(request.RoleId, request.PermissionKeys, ct);

        if (!result.HasChanges || affectedAccountIds.Count == 0)
            return;

        // Different affected Accounts can end up with different effective sets (extra Roles/
        // Positions beyond this one), so each Account's permissions are resolved individually -
        // in one batched query, not one query per Account - and carried in a single Outbox message.
        var effectivePermissionsByAccount = await effectivePermissionReadService
            .GetEffectivePermissionsForAccountsAsync(affectedAccountIds, tenantId, ct);

        var accountUpdates = affectedAccountIds
            .Select(accountId => new AccountEffectivePermissions(
                accountId,
                effectivePermissionsByAccount.TryGetValue(accountId, out var permissions) ? [.. permissions] : []))
            .ToArray();

        await outboxStore.EnqueueAsync(
            new AccountEffectivePermissionsChangedIntegrationEvent(tenantId, accountUpdates),
            ct);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
