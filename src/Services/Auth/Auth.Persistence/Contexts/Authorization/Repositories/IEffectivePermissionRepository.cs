namespace NovaCore.Auth.Persistence.Contexts.Authorization.Repositories;

/// <summary>
/// The data-access side of "effective permissions" - not entity-scoped (it joins UserRoles,
/// AccountPositions, PositionRoles and PermissionGrants in one query per method), so it does not
/// implement the generic IRepository&lt;TEntity&gt; any other repository in this project does.
/// Exists so EffectivePermissionReadService depends on a repository-shaped abstraction rather
/// than AuthDbContext directly, matching every other Read/Write service in Auth.Persistence.
/// </summary>
public interface IEffectivePermissionRepository
{
    Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(
        Guid accountId,
        Guid tenantId,
        CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, IReadOnlySet<string>>> GetEffectivePermissionsForAccountsAsync(
        IReadOnlyCollection<Guid> accountIds,
        Guid tenantId,
        CancellationToken ct = default);

    Task<IReadOnlySet<Guid>> GetAccountIdsForRoleAsync(
        Guid roleId,
        Guid tenantId,
        CancellationToken ct = default);
}
