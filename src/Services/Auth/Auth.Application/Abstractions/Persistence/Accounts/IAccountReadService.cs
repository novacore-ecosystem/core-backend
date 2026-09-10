namespace NovaCore.Auth.Application.Abstractions.Persistence.Accounts;

public interface IAccountReadService
{
    Task<Account?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Tenant-scoped lookup, used before a tenant claim exists (e.g. Login).
    /// </summary>
    /// <remarks>
    /// <paramref name="tenantId"/> is the already-resolved value from the TenantClient's PublicKey
    /// (Guid.Empty for the Root client), not the ambient RequestContext.
    /// </remarks>
    Task<Account?> GetByEmailAsync(string email, Guid tenantId, CancellationToken ct = default);

    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets the RoleIds directly assigned to this account.
    /// </summary>
    /// <remarks>
    /// Direct (AccountRole) assignments only - excludes Position-derived roles.
    /// </remarks>
    /// <param name="accountId">The account to look up.</param>
    Task<IReadOnlySet<Guid>> GetRoleIdsAsync(Guid accountId, CancellationToken ct = default);
}
