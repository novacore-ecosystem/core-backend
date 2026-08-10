using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Persistence.Engine;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.Auth.Persistence.Contexts.Tenants.Read;

public sealed class TenantReadService(AuthDbContext dbContext) : ITenantReadService
{
    public async Task<Tenant?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Code.Value == code, ct);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
    {
        return await dbContext.Tenants
            .AsNoTracking()
            .AnyAsync(t => t.Code.Value == code, ct);
    }
}
