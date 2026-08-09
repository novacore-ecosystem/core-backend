namespace NovaCore.Auth.Application.Abstractions.Persistence.Accounts;

public interface IAccountReadService
{
    Task<Account?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>Tenant-scoped lookup - the Login path (see LoginHandler). TenantId is the already-
    /// resolved value from the TenantClient's PublicKey (Guid.Empty for the Root client), not the
    /// ambient RequestContext, since no tenant claim exists yet at login time.</summary>
    Task<Account?> GetByEmailAsync(string email, Guid tenantId, CancellationToken ct = default);
}
