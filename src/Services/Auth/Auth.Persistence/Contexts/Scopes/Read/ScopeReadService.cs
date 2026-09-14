using NovaCore.Auth.Application.Abstractions.Persistence.Scopes;
using NovaCore.Auth.Domain.Entities.Scopes;
using NovaCore.Auth.Persistence.Contexts.Scopes.Repositories;

namespace NovaCore.Auth.Persistence.Contexts.Scopes.Read;

public sealed class ScopeReadService(IScopeRepository scopeRepo) : IScopeReadService
{
    public async Task<Scope?> GetByCodeAsync(Guid tenantId, string code, CancellationToken ct = default)
    {
        return await scopeRepo.GetAsync(s => s.TenantId == tenantId && s.Code.Value == code, ct);
    }

    public async Task<bool> ExistsByCodeAsync(Guid tenantId, string code, CancellationToken ct = default)
    {
        return await scopeRepo.ExistsAsync(s => s.TenantId == tenantId && s.Code.Value == code, ct);
    }
}
