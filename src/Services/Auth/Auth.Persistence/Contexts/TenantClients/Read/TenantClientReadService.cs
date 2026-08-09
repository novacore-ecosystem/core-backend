using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Application.Abstractions.Persistence.TenantClients;
using NovaCore.Auth.Domain.Entities.TenantClients;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.TenantClients.Read;

public sealed class TenantClientReadService(AuthDbContext dbContext) : ITenantClientReadService, IPersistenceService
{
    public async Task<TenantClient?> GetByPublicKeyAsync(string publicKey, CancellationToken ct = default)
    {
        return await dbContext.TenantClients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.PublicKey.Value == publicKey, ct);
    }

    public async Task<bool> ExistsByPublicKeyAsync(string publicKey, CancellationToken ct = default)
    {
        return await dbContext.TenantClients
            .AsNoTracking()
            .AnyAsync(c => c.PublicKey.Value == publicKey, ct);
    }
}
