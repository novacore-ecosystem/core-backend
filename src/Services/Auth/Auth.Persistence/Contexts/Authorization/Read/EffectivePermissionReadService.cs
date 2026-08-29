using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Domain.Enums;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Persistence;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.Auth.Persistence.Contexts.Authorization.Read;

/// <summary>The reference Persistence Service for this codebase - a meaningful, cross-aggregate
/// authorization-data concern (Role/Position/PermissionGrant), not a Repository-method decorator.
/// IPersistenceService makes it auto-registered (see Auth.Persistence/DependencyInjection.cs).</summary>
public sealed class EffectivePermissionReadService(AuthDbContext dbContext) : IEffectivePermissionReadService, IPersistenceService
{
    public async Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(Guid accountId, Guid tenantId, CancellationToken ct = default)
    {
        var directRoleIds = dbContext.UserRoles
            .Where(ar => ar.UserId == accountId)
            .Select(ar => ar.RoleId);

        // AccountPosition/PositionRole/PermissionGrant are all ITenantEntity, so their automatic
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

        var roleIds = await directRoleIds.Union(positionRoleIds).ToListAsync(ct);
        if (roleIds.Count == 0)
            return new HashSet<string>(StringComparer.Ordinal);

        // PermissionGrant.ProviderKey is a generic string (it also has to hold non-Guid provider
        // keys like a future Guest "*"), so the Role -> grant join happens against materialized
        // string keys rather than relying on SQL-side Guid-to-text translation.
        var roleProviderKeys = roleIds.Select(id => id.ToString()).ToArray();

        var permissionKeys = await dbContext.PermissionGrants
            .IgnoreQueryFilters()
            .Where(g => g.TenantId == tenantId
                && g.ProviderName == PermissionProviderName.Role
                && roleProviderKeys.Contains(g.ProviderKey))
            .Select(g => g.PermissionDefinition.Key.Value)
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

        // accountIds.Contains(...) translates to a single "= ANY(@p)" array parameter on Npgsql,
        // not one bound parameter per id, so this stays two queries total regardless of how many
        // affected accounts a Role update touches - not one query per account.
        var accountRoleGrants = await directGrants.Union(positionGrants).ToListAsync(ct);
        if (accountRoleGrants.Count == 0)
            return new Dictionary<Guid, IReadOnlySet<string>>();

        // See GetEffectivePermissionsAsync - ProviderKey is a generic string, so the join happens
        // against materialized string keys, not SQL-side Guid-to-text translation.
        var roleProviderKeys = accountRoleGrants.Select(g => g.RoleId.ToString()).Distinct().ToArray();

        var grants = await dbContext.PermissionGrants
            .IgnoreQueryFilters()
            .Where(g => g.TenantId == tenantId
                && g.ProviderName == PermissionProviderName.Role
                && roleProviderKeys.Contains(g.ProviderKey))
            .Select(g => new { g.ProviderKey, PermissionKey = g.PermissionDefinition.Key.Value })
            .ToListAsync(ct);

        var permissionKeysByRoleId = grants
            .GroupBy(g => g.ProviderKey, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Select(x => x.PermissionKey).ToArray(), StringComparer.Ordinal);

        return accountRoleGrants
            .GroupBy(g => g.AccountId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlySet<string>)g
                    .SelectMany(x => permissionKeysByRoleId.TryGetValue(x.RoleId.ToString(), out var keys) ? keys : [])
                    .ToHashSet(StringComparer.Ordinal));
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
