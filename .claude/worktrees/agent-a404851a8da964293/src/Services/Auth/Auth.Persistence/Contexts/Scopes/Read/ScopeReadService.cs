using NovaCore.Auth.Application.Abstractions.Persistence.Scopes;
using NovaCore.Auth.Domain.Entities.Scopes;
using NovaCore.Auth.Persistence.Engine;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.Auth.Persistence.Contexts.Scopes.Read;

public sealed class ScopeReadService(AuthDbContext dbContext) : IScopeReadService
{
    public async Task<Scope?> GetByCodeAsync(Guid tenantId, string code, CancellationToken ct = default)
    {
        return await dbContext.Scopes
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Code.Value == code, ct);
    }

    public async Task<bool> ExistsByCodeAsync(Guid tenantId, string code, CancellationToken ct = default)
    {
        return await dbContext.Scopes
            .AsNoTracking()
            .AnyAsync(s => s.TenantId == tenantId && s.Code.Value == code, ct);
    }
}
