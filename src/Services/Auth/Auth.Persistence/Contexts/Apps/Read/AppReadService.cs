using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Application.Abstractions.Persistence.Apps;
using NovaCore.Auth.Domain.Entities.Apps;
using NovaCore.Auth.Domain.ValueObjects;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Apps.Read;

public sealed class AppReadService(AuthDbContext dbContext) : IAppReadService, IPersistenceService
{
    public async Task<App?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Apps
            .AsNoTracking()
            .Include(a => a.Translations)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<App?> GetByCodeAsync(AppCode code, CancellationToken ct = default)
    {
        return await dbContext.Apps
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Code.Equals(code), ct);
    }

    public async Task<bool> ExistsByCodeAsync(AppCode code, CancellationToken ct = default)
    {
        return await dbContext.Apps
            .AsNoTracking()
            .AnyAsync(a => a.Code.Equals(code), ct);
    }

    public async Task<(IReadOnlyList<App> Items, int TotalCount)> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = dbContext.Apps.AsNoTracking();

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
}
