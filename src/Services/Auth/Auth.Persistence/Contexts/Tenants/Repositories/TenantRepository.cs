using Microsoft.EntityFrameworkCore;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.Tenants.Repositories;

public sealed class TenantRepository(AuthDbContext dbContext)
    : AuthBaseRepository<Tenant>(dbContext), ITenantRepository
{
    public async Task<(int Version, bool IsActive)?> GetVersionAsync(Guid id, CancellationToken ct = default)
    {
        var result = await _dbSet
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new { t.Version, t.IsActive })
            .FirstOrDefaultAsync(ct);

        return result is null ? null : (result.Version, result.IsActive);
    }

    public async Task<(IReadOnlyList<Tenant> Items, int TotalCount)> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking();

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
