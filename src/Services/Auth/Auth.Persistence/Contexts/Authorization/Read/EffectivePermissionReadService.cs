using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Domain.Enums;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Authorization.Read;

/// <summary>The reference Persistence Service for this codebase - a meaningful, cross-aggregate
/// authorization-data concern (Role/Position/RolePermission), not a Repository-method decorator.
/// IPersistenceService makes it auto-registered (see Auth.Persistence/DependencyInjection.cs).</summary>
public sealed class EffectivePermissionReadService(AuthDbContext dbContext) : IEffectivePermissionReadService, IPersistenceService
{
    public async Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(Guid accountId, Guid tenantId, CancellationToken ct = default)
    {
        var directRoleIds = dbContext.UserRoles
            .Where(ar => ar.UserId == accountId)
            .Select(ar => ar.RoleId);

        // AccountPosition/PositionRole/RolePermission are all ITenantEntity, so their automatic
        // query filter compares against RequestContext.Current.TenantId - Guid.Empty at Login
        // time, since no tenant claim exists yet. IgnoreQueryFilters + an explicit TenantId
        // equality is the deliberate escape hatch for that (same reasoning as Phase 2's
        // Account/TenantClient lookups, which sidestepped it by not implementing ITenantEntity at
        // all - these entities correctly stay ITenantEntity for every other, post-auth context).
        var now = DateTime.UtcNow;
        var positionRoleIds = dbContext.AccountPositions
            .IgnoreQueryFilters()
            .Where(ap => ap.AccountId == accountId
                && ap.TenantId == tenantId
                && ap.Status == AccountPositionStatus.Active
                && (ap.ExpiredAt == null || ap.ExpiredAt > now))
            .Join(
                dbContext.PositionRoles.IgnoreQueryFilters().Where(pr => pr.TenantId == tenantId),
                ap => ap.PositionId,
                pr => pr.PositionId,
                (ap, pr) => pr.RoleId);

        var roleIds = directRoleIds.Union(positionRoleIds);

        var permissionKeys = await dbContext.RolePermissions
            .IgnoreQueryFilters()
            .Where(rp => rp.TenantId == tenantId && roleIds.Contains(rp.RoleId))
            .Select(rp => rp.PermissionDefinition.Key.Value)
            .Distinct()
            .ToListAsync(ct);

        return permissionKeys.ToHashSet(StringComparer.Ordinal);
    }

    public async Task<IReadOnlyDictionary<Guid, IReadOnlySet<string>>> GetEffectivePermissionsForAccountsAsync(
        IReadOnlyCollection<Guid> accountIds, Guid tenantId, CancellationToken ct = default)
    {
        if (accountIds.Count == 0)
            return new Dictionary<Guid, IReadOnlySet<string>>();

        var directGrants = dbContext.UserRoles
            .Where(ar => accountIds.Contains(ar.UserId))
            .Select(ar => new { AccountId = ar.UserId, ar.RoleId });

        var now = DateTime.UtcNow;
        var positionGrants = dbContext.AccountPositions
            .IgnoreQueryFilters()
            .Where(ap => accountIds.Contains(ap.AccountId)
                && ap.TenantId == tenantId
                && ap.Status == AccountPositionStatus.Active
                && (ap.ExpiredAt == null || ap.ExpiredAt > now))
            .Join(
                dbContext.PositionRoles.IgnoreQueryFilters().Where(pr => pr.TenantId == tenantId),
                ap => ap.PositionId,
                pr => pr.PositionId,
                (ap, pr) => new { AccountId = ap.AccountId, pr.RoleId });

        var accountRoleGrants = directGrants.Union(positionGrants);

        // accountIds.Contains(...) translates to a single "= ANY(@p)" array parameter on Npgsql,
        // not one bound parameter per id, so this stays one query regardless of how many affected
        // accounts a Role update touches.
        var rows = await accountRoleGrants
            .Join(
                dbContext.RolePermissions.IgnoreQueryFilters().Where(rp => rp.TenantId == tenantId),
                arg => arg.RoleId,
                rp => rp.RoleId,
                (arg, rp) => new { arg.AccountId, PermissionKey = rp.PermissionDefinition.Key.Value })
            .Distinct()
            .ToListAsync(ct);

        return rows
            .GroupBy(r => r.AccountId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlySet<string>)g.Select(r => r.PermissionKey).ToHashSet(StringComparer.Ordinal));
    }

    public async Task<IReadOnlySet<Guid>> GetAccountIdsForRoleAsync(Guid roleId, Guid tenantId, CancellationToken ct = default)
    {
        var directAccountIds = dbContext.UserRoles
            .Where(ar => ar.RoleId == roleId)
            .Join(
                dbContext.Users.Where(u => u.TenantId == tenantId),
                ar => ar.UserId,
                u => u.Id,
                (ar, u) => u.Id);

        var now = DateTime.UtcNow;
        var positionAccountIds = dbContext.PositionRoles
            .IgnoreQueryFilters()
            .Where(pr => pr.RoleId == roleId && pr.TenantId == tenantId)
            .Join(
                dbContext.AccountPositions
                    .IgnoreQueryFilters()
                    .Where(ap => ap.TenantId == tenantId
                        && ap.Status == AccountPositionStatus.Active
                        && (ap.ExpiredAt == null || ap.ExpiredAt > now)),
                pr => pr.PositionId,
                ap => ap.PositionId,
                (pr, ap) => ap.AccountId);

        var accountIds = await directAccountIds.Union(positionAccountIds).ToListAsync(ct);
        return accountIds.ToHashSet();
    }
}
