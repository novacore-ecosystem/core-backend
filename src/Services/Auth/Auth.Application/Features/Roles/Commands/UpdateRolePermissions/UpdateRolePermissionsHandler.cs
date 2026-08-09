using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;
using NovaCore.Auth.Application.Abstractions.Persistence.Roles;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Contract.Events.User;
using NovaCore.BuildingBlock.SharedKernel.Context;

namespace NovaCore.Auth.Application.Features.Roles.Commands.UpdateRolePermissions;

/// <summary>
/// Two-phase commit, not one atomic transaction: the Role/RolePermission mutation commits first
/// (RoleWriteService self-commits, matching every other Write Service in Auth), then affected
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
    IPermissionReadService permissionReadService,
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

        var requestedKeys = request.PermissionKeys.ToHashSet(StringComparer.Ordinal);
        var allPermissions = await permissionReadService.ListAsync(ct);
        var permissionsByKey = allPermissions.ToDictionary(p => p.Key.Value, StringComparer.Ordinal);

        await roleWriteService.UpdateWithPermissionsAsync(request.RoleId, role =>
        {
            var currentKeys = role.Permissions
                .Select(rp => rp.PermissionDefinition.Key.Value)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var key in requestedKeys.Except(currentKeys))
            {
                if (permissionsByKey.TryGetValue(key, out var permission))
                    role.AssignPermission(permission);
            }

            foreach (var permissionDefinitionId in role.Permissions
                .Where(rp => !requestedKeys.Contains(rp.PermissionDefinition.Key.Value))
                .Select(rp => rp.PermissionDefinitionId)
                .ToList())
            {
                role.RemovePermission(permissionDefinitionId);
            }
        }, ct);

        foreach (var accountId in affectedAccountIds)
        {
            var permissions = await effectivePermissionReadService.GetEffectivePermissionsAsync(accountId, tenantId, ct);
            await outboxStore.EnqueueAsync(
                new AccountEffectivePermissionsChangedIntegrationEvent(tenantId, accountId, [.. permissions]),
                ct);
        }

        if (affectedAccountIds.Count > 0)
            await unitOfWork.SaveChangesAsync(ct);
    }
}
