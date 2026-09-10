using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.Persistence;

// Auth.Persistence.Contexts.Permissions is a sibling namespace of this file's own
// (Auth.Persistence.Contexts.Authorization) - an unqualified "Permissions" resolves to that
// namespace instead of the Permissions catalog class (and shadows a same-named using-alias too),
// so the catalog is aliased under a different name here to avoid the collision entirely.
using PermissionCatalog = NovaCore.BuildingBlock.SharedKernel.Constants.Permissions;

namespace NovaCore.Auth.Persistence.Contexts.Authorization;

/// <summary>The reference Persistence Service for cross-cutting authorization-scope checks - see
/// IAccountAuthorizationGuard's own doc comment for the rules enforced. Injects AuthDbContext
/// directly for the same reason EffectivePermissionReadService/PermissionGrantService do: this is a
/// cross-aggregate concern (Account, AccountRole, PermissionGrant), not a single-entity Repository
/// operation.</summary>
public sealed class AccountAuthorizationGuard(
    AuthDbContext dbContext,
    IEffectivePermissionReadService effectivePermissionReadService) : IAccountAuthorizationGuard, IPersistenceService
{
    public async Task EnsureCanManageAccountAsync(Guid actorId, Guid targetAccountId, Guid tenantId, CancellationToken ct = default)
    {
        var actorPermissions = await effectivePermissionReadService.GetEffectivePermissionsAsync(actorId, tenantId, ct);
        if (actorPermissions.Contains(PermissionCatalog.Root))
            return;

        var targetPermissions = await effectivePermissionReadService.GetEffectivePermissionsAsync(targetAccountId, tenantId, ct);
        if (targetPermissions.Contains(PermissionCatalog.Root))
            throw new ForbiddenException("A Root account cannot be managed through this operation.");

        var actorLevel = await GetLevelAsync(actorId, ct);
        var targetLevel = await GetLevelAsync(targetAccountId, ct);
        if (actorLevel <= targetLevel)
            throw new ForbiddenException("You do not have sufficient authority to manage this account.");
    }

    public async Task EnsureCanGrantPermissionsAsync(Guid actorId, IReadOnlyCollection<string> permissionKeys, Guid tenantId, CancellationToken ct = default)
    {
        if (permissionKeys.Count == 0)
            return;

        var actorPermissions = await effectivePermissionReadService.GetEffectivePermissionsAsync(actorId, tenantId, ct);
        if (actorPermissions.Contains(PermissionCatalog.Root))
            return;

        var missing = permissionKeys.Where(key => !actorPermissions.Contains(key)).ToArray();
        if (missing.Length > 0)
            throw new ForbiddenException(
                $"You cannot grant a permission you do not hold: {string.Join(", ", missing)}.");
    }

    public async Task EnsureCanGrantRoleAsync(Guid actorId, Guid roleId, Guid tenantId, CancellationToken ct = default)
    {
        var actorPermissions = await effectivePermissionReadService.GetEffectivePermissionsAsync(actorId, tenantId, ct);
        if (actorPermissions.Contains(PermissionCatalog.Root))
            return;

        var actorHoldsRole = await dbContext.UserRoles
            .AnyAsync(ar => ar.UserId == actorId && ar.RoleId == roleId, ct);
        if (!actorHoldsRole)
            throw new ForbiddenException("You cannot assign a role you do not hold.");
    }

    private async Task<int> GetLevelAsync(Guid accountId, CancellationToken ct)
    {
        var level = await dbContext.Users
            .Where(u => u.Id == accountId)
            .Select(u => (int?)u.Level)
            .FirstOrDefaultAsync(ct);

        return level ?? throw ExceptionFactory.EntityNotFound<Domain.Entities.Accounts.Account>(accountId);
    }
}
