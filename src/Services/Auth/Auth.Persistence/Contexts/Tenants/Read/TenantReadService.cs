using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Domain.ValueObjects;
using NovaCore.Auth.Persistence.Contexts.Tenants.Repositories;
using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Tenants.Read;

public sealed class TenantReadService(ITenantRepository tenantRepo) : ITenantReadService, IPersistenceService
{
    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await tenantRepo.GetAsync(
            t => t.Id == id,
            q => q.Include(t => t.Translations),
            ct);
    }

    public async Task<(int Version, bool IsActive)?> GetVersionAsync(Guid id, CancellationToken ct = default)
    {
        return await tenantRepo.GetVersionAsync(id, ct);
    }

    public async Task<Tenant?> GetByCodeAsync(TenantCode code, CancellationToken ct = default)
    {
        return await tenantRepo.GetAsync(t => t.Code.Equals(code), ct);
    }

    public async Task<bool> ExistsByCodeAsync(TenantCode code, CancellationToken ct = default)
    {
        return await tenantRepo.ExistsAsync(t => t.Code.Equals(code), ct);
    }

    public async Task<(IReadOnlyList<Tenant> Items, int TotalCount)> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        return await tenantRepo.SearchAsync(search, page, pageSize, ct);
    }
}
