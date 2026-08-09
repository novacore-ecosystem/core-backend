using NovaCore.Auth.Domain.Entities.TenantClients;
using NovaCore.Auth.Persistence.Engine;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.Auth.Persistence.Storage.Seeders;

/// <summary>
/// Seeds the one Root TenantClient (TenantId == null - see TenantClient's class doc comment) so
/// Login has a usable Root public key on a fresh environment, the same way AccountSeeder/
/// RoleSeeder guarantee a usable Root account/role without a normal registration flow. Returns the
/// newly created client (null if one already existed) so the caller can log its PublicKey - there
/// is no other bootstrap channel for it yet (no API exposes it, it cannot be predicted or reset),
/// same local-dev-only tradeoff SeedData.Accounts.RootPassword already accepts.
/// </summary>
public class TenantClientSeeder(AuthDbContext context)
{
    public async Task<TenantClient?> SeedAsync()
    {
        if (await context.TenantClients.AnyAsync(c => c.TenantId == null))
            return null;

        var rootClient = TenantClient.Create(tenantId: null, name: "Root");
        await context.TenantClients.AddAsync(rootClient);
        await context.SaveChangesAsync();

        return rootClient;
    }
}
