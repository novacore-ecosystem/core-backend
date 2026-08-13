using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Application.Abstractions.Persistence.TenantClients;
using NovaCore.Auth.Domain.Entities.TenantClients;
using NovaCore.Auth.Domain.ValueObjects;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.TenantClients.Read;

public sealed class TenantClientReadService(AuthDbContext dbContext) : ITenantClientReadService, IPersistenceService
{
    public async Task<TenantClient?> GetByPublicKeyAsync(string publicKey, CancellationToken ct = default)
    {
        // Compare the value object itself, not c.PublicKey.Value - TenantClientConfig maps
        // PublicKey via HasConversion, which EF can translate for an Equal on the mapped property
        // directly, but not for an arbitrary member access (.Value) inside the expression tree.
        var key = ClientPublicKey.Create(publicKey);
        return await dbContext.TenantClients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.PublicKey == key, ct);
    }

    public async Task<bool> ExistsByPublicKeyAsync(string publicKey, CancellationToken ct = default)
    {
        var key = ClientPublicKey.Create(publicKey);
        return await dbContext.TenantClients
            .AsNoTracking()
            .AnyAsync(c => c.PublicKey == key, ct);
    }
}
