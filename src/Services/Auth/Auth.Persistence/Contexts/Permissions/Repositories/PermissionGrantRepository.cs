using Microsoft.EntityFrameworkCore;
using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.Auth.Persistence.Contexts;
using NovaCore.Auth.Persistence.Engine;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.Auth.Persistence.Contexts.Permissions.Repositories;

public sealed class PermissionGrantRepository(AuthDbContext dbContext)
    : AuthBaseRepository<PermissionGrant>(dbContext), IPermissionGrantRepository
{
    public async Task<bool> ExistsForProviderAsync(
        Guid tenantId,
        Guid permissionDefinitionId,
        PermissionProviderName providerName,
        string providerKey,
        CancellationToken ct = default)
    {
        return await _dbSet
            .IgnoreQueryFilters()
            .AnyAsync(
                g => g.TenantId == tenantId
                    && g.PermissionDefinitionId == permissionDefinitionId
                    && g.ProviderName == providerName
                    && g.ProviderKey == providerKey,
                ct);
    }

    public async Task<PermissionGrant?> GetForProviderAsync(
        Guid tenantId,
        string permissionKey,
        PermissionProviderName providerName,
        string providerKey,
        CancellationToken ct = default)
    {
        return await _dbSet
            .IgnoreQueryFilters()
            .Include(g => g.PermissionDefinition)
            .FirstOrDefaultAsync(
                g => g.TenantId == tenantId
                    && g.PermissionDefinition.Key.Value == permissionKey
                    && g.ProviderName == providerName
                    && g.ProviderKey == providerKey,
                ct);
    }

    public async Task<List<PermissionGrant>> ListForProviderAsync(
        Guid tenantId,
        PermissionProviderName providerName,
        string providerKey,
        CancellationToken ct = default)
    {
        return await _dbSet
            .IgnoreQueryFilters()
            .Include(g => g.PermissionDefinition)
            .Where(g => g.TenantId == tenantId && g.ProviderName == providerName && g.ProviderKey == providerKey)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlySet<string>> GetGrantedKeysAsync(
        Guid tenantId,
        PermissionProviderName providerName,
        string providerKey,
        CancellationToken ct = default)
    {
        var keys = await _dbSet
            .IgnoreQueryFilters()
            .Where(g => g.TenantId == tenantId && g.ProviderName == providerName && g.ProviderKey == providerKey)
            .Select(g => g.PermissionDefinition.Key.Value)
            .ToListAsync(ct);

        return keys.ToHashSet(StringComparer.Ordinal);
    }

    public async Task<IReadOnlyList<string>> GetKeysByProviderAsync(
        PermissionProviderName providerName,
        string providerKey,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(g => g.ProviderName == providerName && g.ProviderKey == providerKey)
            .Select(g => g.PermissionDefinition.Key.Value)
            .ToListAsync(ct);
    }

    public Task RemoveRangeAsync(IEnumerable<PermissionGrant> grants, CancellationToken ct = default)
    {
        _dbSet.RemoveRange(grants);
        return Task.CompletedTask;
    }
}
