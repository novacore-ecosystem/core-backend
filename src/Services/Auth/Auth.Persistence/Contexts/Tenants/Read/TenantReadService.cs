using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Persistence.Engine;
using NovaCore.BuildingBlock.Persistence;
using NovaCore.Auth.Domain.ValueObjects;

namespace NovaCore.Auth.Persistence.Contexts.Tenants.Read;

public sealed class TenantReadService(AuthDbContext dbContext) : ITenantReadService, IPersistenceService
{
    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Tenants
            .AsNoTracking()
            .Include(t => t.Locales)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<(int Version, bool IsActive)?> GetVersionAsync(Guid id, CancellationToken ct = default)
    {
        var result = await dbContext.Tenants
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new { t.Version, t.IsActive })
            .FirstOrDefaultAsync(ct);

        return result is null ? null : (result.Version, result.IsActive);
    }

    public async Task<Tenant?> GetByCodeAsync(TenantCode code, CancellationToken ct = default)
    {
        return await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Code.Equals(code), ct);
    }

    public async Task<bool> ExistsByCodeAsync(TenantCode code, CancellationToken ct = default)
    {
        return await dbContext.Tenants
            .AsNoTracking()
            .AnyAsync(t => t.Code.Equals(code), ct);
    }

    public async Task<(IReadOnlyList<Tenant> Items, int TotalCount)> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = dbContext.Tenants.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(t =>
                EF.Functions.ILike(t.Name, pattern) ||
                EF.Functions.ILike(t.Code.Value, pattern));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
