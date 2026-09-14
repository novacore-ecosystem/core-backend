using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Persistence.Contexts.Authorization.Repositories;
using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Authorization.Read;

public sealed class EffectivePermissionReadService(IEffectivePermissionRepository effectivePermissionRepo)
    : IEffectivePermissionReadService, IPersistenceService
{
    public Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(
        Guid accountId,
        Guid tenantId,
        CancellationToken ct = default)
    {
        return effectivePermissionRepo.GetEffectivePermissionsAsync(accountId, tenantId, ct);
    }

    public Task<IReadOnlyDictionary<Guid, IReadOnlySet<string>>> GetEffectivePermissionsForAccountsAsync(
        IReadOnlyCollection<Guid> accountIds, Guid tenantId, CancellationToken ct = default)
    {
        return effectivePermissionRepo.GetEffectivePermissionsForAccountsAsync(accountIds, tenantId, ct);
    }

    public Task<IReadOnlySet<Guid>> GetAccountIdsForRoleAsync(
        Guid roleId,
        Guid tenantId,
        CancellationToken ct = default)
    {
        return effectivePermissionRepo.GetAccountIdsForRoleAsync(roleId, tenantId, ct);
    }
}
