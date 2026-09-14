using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;
using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.Auth.Infrastructure.Authorization;

/// <summary>
/// Composes the guard, the effective-authorization cache and the Role/PermissionGrant write paths
/// into the three Account authorization-management operations.
/// </summary>
/// <remarks>
/// Resolves the actor's and target's <see cref="AccountAuthorizationSnapshot"/> once per operation
/// (cache-first) and reuses them across every <see cref="AccountAuthorizationGuard"/> check, instead
/// of each check re-resolving effective permissions independently.
/// Non-committing beyond its own flush calls - <see cref="ReplaceRolesAsync"/> and
/// <see cref="ReplacePermissionsAsync"/> each perform a write plus an Outbox enqueue that must
/// stay atomic, so their callers (<c>ReplaceAccountRolesHandler</c>/<c>ReplaceAccountPermissionsHandler</c>)
/// own the surrounding <c>IUnitOfWork.ExecuteTransactionAsync</c> transaction, not this service.
/// </remarks>
public sealed class AccountAuthorizationService(
    IAccountReadService accountReadService,
    IAccountRoleAssignmentService accountRoleAssignmentService,
    IPermissionGrantService permissionGrantService,
    IAccountWriteService accountWriteService,
    ITenantReadService tenantReadService,
    IEffectiveAuthorizationCache authorizationCache,
    IOutboxStore outboxStore,
    IUnitOfWork unitOfWork) : IAccountAuthorizationService
{
    public async Task ReplaceRolesAsync(
        Guid actorId,
        Guid accountId,
        IReadOnlyCollection<Guid> roleIds,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var actor = await authorizationCache.GetAsync(actorId, tenantId, ct);
        var target = await authorizationCache.GetAsync(accountId, tenantId, ct);
        AccountAuthorizationGuard.EnsureCanManageAccount(actor, target);

        var currentRoleIds = await accountReadService.GetRoleIdsAsync(accountId, ct);
        var newlyAddedRoleIds = roleIds
            .Where(id => !currentRoleIds.Contains(id))
            .ToArray();
        await EnsureCanGrantNewRolesAsync(actor, actorId, newlyAddedRoleIds, ct);

        var result = await accountRoleAssignmentService.ReplaceRolesAsync(accountId, roleIds, ct);
        if (result.HasChanges)
            await PropagateEffectivePermissionsAsync(accountId, tenantId, ct);
    }

    public async Task ReplacePermissionsAsync(
        Guid actorId,
        Guid accountId,
        IReadOnlyCollection<string> permissionKeys,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var actor = await authorizationCache.GetAsync(actorId, tenantId, ct);
        var target = await authorizationCache.GetAsync(accountId, tenantId, ct);
        AccountAuthorizationGuard.EnsureCanManageAccount(actor, target);

        var providerKey = accountId.ToString();
        var currentKeys = await permissionGrantService.GetGrantedKeysAsync(
            PermissionProviderName.User, providerKey, tenantId, ct);
        var newlyAddedKeys = permissionKeys.Where(key => !currentKeys.Contains(key)).ToArray();
        AccountAuthorizationGuard.EnsureCanGrantPermissions(actor, newlyAddedKeys);
        await EnsureWithinTenantBoundaryAsync(actor, tenantId, newlyAddedKeys, ct);

        var result = await permissionGrantService.ReplaceForProviderAsync(
            PermissionProviderName.User, providerKey, permissionKeys, tenantId, ct);
        if (result.HasChanges)
            await PropagateEffectivePermissionsAsync(accountId, tenantId, ct);
    }

    public async Task SetLevelAsync(
        Guid actorId,
        Guid accountId,
        int level,
        Guid tenantId,
        CancellationToken ct = default)
    {
        // Not Root-bypassed, unlike every other rule here - an account must never be able to
        // change its own level through this operation.
        if (actorId == accountId)
            throw new ForbiddenException("You cannot change your own account level.");

        var actor = await authorizationCache.GetAsync(actorId, tenantId, ct);
        var target = await authorizationCache.GetAsync(accountId, tenantId, ct);
        AccountAuthorizationGuard.EnsureCanManageAccount(actor, target);

        if (actor.Level <= level)
            throw new ForbiddenException("You cannot grant a level you do not outrank.");

        await accountWriteService.SetLevelAsync(accountId, level, ct);
        await authorizationCache.InvalidateAsync(accountId, tenantId, ct);
    }

    /// <summary>
    /// Shared by the Account permission path here and by UpdateRolePermissionsHandler (the Role
    /// permission path, Auth.Application) - both resolve the same tenant boundary before applying
    /// a permission diff, so the rule lives once in AccountAuthorizationGuard and each caller only
    /// wires up its own tenant/allowed-keys lookup.
    /// </summary>
    private async Task EnsureWithinTenantBoundaryAsync(
        AccountAuthorizationSnapshot actor,
        Guid tenantId,
        IReadOnlyCollection<string> newlyAddedKeys,
        CancellationToken ct)
    {
        if (actor.HasRoot || newlyAddedKeys.Count == 0)
            return;

        var tenant = await tenantReadService.GetByIdAsync(tenantId, ct)
            ?? throw ExceptionFactory.EntityNotFound($"Tenant \"{tenantId}\" does not exist.");

        var allowedKeys = await permissionGrantService.GetGrantedKeysAsync(
            PermissionProviderName.Tenant, tenantId.ToString(), tenantId, ct);

        AccountAuthorizationGuard.EnsureWithinTenantBoundary(
            actor, tenant.Metadata.PermissionBoundaryEnabled, allowedKeys, newlyAddedKeys);
    }

    private async Task EnsureCanGrantNewRolesAsync(
        AccountAuthorizationSnapshot actor,
        Guid actorId,
        IReadOnlyCollection<Guid> newlyAddedRoleIds,
        CancellationToken ct)
    {
        if (actor.HasRoot || newlyAddedRoleIds.Count == 0)
            return;

        var actorRoleIds = await accountReadService.GetRoleIdsAsync(actorId, ct);
        foreach (var roleId in newlyAddedRoleIds)
            AccountAuthorizationGuard.EnsureCanGrantRole(actor, actorRoleIds.Contains(roleId));
    }

    private async Task PropagateEffectivePermissionsAsync(
        Guid accountId,
        Guid tenantId,
        CancellationToken ct)
    {
        await authorizationCache.InvalidateAsync(accountId, tenantId, ct);
        var refreshedSnapshot = await authorizationCache.GetAsync(accountId, tenantId, ct);

        await outboxStore.EnqueueAsync(
            new AccountEffectivePermissionsChangedIntegrationEvent(
                tenantId,
                [new AccountEffectivePermissions(accountId, [.. refreshedSnapshot.PermissionKeys])]),
            ct);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
