using Microsoft.EntityFrameworkCore;
using NovaCore.Auth.Domain.Entities.Apps;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.Apps.Repositories;

public sealed class AppRepository(AuthDbContext dbContext)
    : AuthBaseRepository<App>(dbContext), IAppRepository
{
    public async Task<IReadOnlyList<App>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<App> Items, int TotalCount)> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(a => EF.Functions.ILike(a.Name, pattern) || EF.Functions.ILike(a.Code.Value, pattern));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(a => a.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyCollection<Guid>> GetAccountIdsAsync(Guid appId, CancellationToken ct = default)
    {
        return await _dbContext.AccountApps
            .AsNoTracking()
            .Where(aa => aa.AppId == appId)
            .Select(aa => aa.AccountId)
            .ToListAsync(ct);
    }
}
