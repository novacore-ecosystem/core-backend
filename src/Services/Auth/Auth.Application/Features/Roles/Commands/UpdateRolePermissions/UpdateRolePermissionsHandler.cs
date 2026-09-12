using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;
using NovaCore.Auth.Application.Abstractions.Persistence.Roles;
using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Contract.Events.User;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Authorization;
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
/// <remarks>
/// Authorization here mirrors AccountAuthorizationService.ReplacePermissionsAsync's User-provider
/// path (see AccountAuthorizationGuard) - this handler previously enforced none of it, which meant
/// an actor could grant a Role permissions they did not themselves hold, resurrect a Root-granting
/// Role, or exceed their tenant's permission boundary purely because the mutation went through the
/// Role rather than the Account path. The same three checks now run before every Role permission
/// mutation, exactly as they do for Accounts.
/// </remarks>
public sealed class UpdateRolePermissionsHandler(
    IRoleReadService roleReadService,
    IRoleWriteService roleWriteService,
    IPermissionGrantService permissionGrantService,
    ITenantReadService tenantReadService,
    IEffectiveAuthorizationCache authorizationCache,
    IEffectivePermissionReadService effectivePermissionReadService,
    ICurrentUserService currentUserService,
    IOutboxStore outboxStore,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateRolePermissionsCommand>
{
    public async Task Handle(UpdateRolePermissionsCommand request, CancellationToken ct = default)
    {
        var tenantId = RequestContext.Current.TenantId ?? Guid.Empty;
        var actorId = currentUserService.GetUserId() ?? throw new UnauthorizedException();
        var actor = await authorizationCache.GetAsync(actorId, tenantId, ct);

        var currentKeys = await roleReadService.GetPermissionKeysAsync(request.RoleId, ct);
        var currentKeySet = currentKeys.ToHashSet(StringComparer.Ordinal);
        var newlyAddedKeys = request.PermissionKeys.Where(key => !currentKeySet.Contains(key)).ToArray();

        AccountAuthorizationGuard.EnsureTargetDoesNotGrantRoot(actor, currentKeySet);
        AccountAuthorizationGuard.EnsureCanGrantPermissions(actor, newlyAddedKeys);
        await EnsureWithinTenantBoundaryAsync(actor, tenantId, newlyAddedKeys, ct);

        // Role membership (who holds this Role) is unaffected by a permission-set change, so this
        // is safe to resolve before the mutation below.
        var affectedAccountIds = await effectivePermissionReadService.GetAccountIdsForRoleAsync(request.RoleId, tenantId, ct);

        var result = await roleWriteService.UpdatePermissionsAsync(request.RoleId, request.PermissionKeys, tenantId, ct);

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

    /// <summary>Same rule as AccountAuthorizationService.EnsureWithinTenantBoundaryAsync
    /// (Auth.Infrastructure) - duplicated wiring, not duplicated logic; the actual boundary check
    /// lives once in AccountAuthorizationGuard.EnsureWithinTenantBoundary.</summary>
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
}
