namespace NovaCore.Auth.Application.Abstractions.Persistence.Accounts;

public interface IAccountReadService
{
    Task<Account?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>Tenant-scoped lookup - the Login path (see LoginHandler). TenantId is the already-
    /// resolved value from the TenantClient's PublicKey (Guid.Empty for the Root client), not the
    /// ambient RequestContext, since no tenant claim exists yet at login time.</summary>
    Task<Account?> GetByEmailAsync(string email, Guid tenantId, CancellationToken ct = default);

    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>The RoleIds currently directly assigned to this Account (AccountRole), not
    /// including Position-derived ones - used to diff against a requested role set before
    /// applying/validating a change (see IAccountAuthorizationGuard/ReplaceAccountRolesHandler).</summary>
    Task<IReadOnlySet<Guid>> GetRoleIdsAsync(Guid accountId, CancellationToken ct = default);
}
