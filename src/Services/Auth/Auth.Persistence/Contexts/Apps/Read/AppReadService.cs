using NovaCore.Auth.Application.Abstractions.Persistence.Apps;
using NovaCore.Auth.Domain.Entities.Apps;
using NovaCore.Auth.Domain.ValueObjects;
using NovaCore.Auth.Persistence.Contexts.Apps.Repositories;
using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Apps.Read;

public sealed class AppReadService(IAppRepository appRepo)
    : IAppReadService, IPersistenceService
{
    public async Task<App?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        return await appRepo.GetAsync(
            app => app.Id == id,
            q => q.Include(app => app.Translations),
            ct);
    }

    public async Task<App?> GetByCodeAsync(
        AppCode code,
        CancellationToken ct = default)
    {
        return await appRepo.GetAsync(
            app => app.Code.Equals(code),
            ct);
    }

    public async Task<bool> ExistsByCodeAsync(
        AppCode code,
        CancellationToken ct = default)
    {
        return await appRepo.ExistsAsync(
            app => app.Code.Equals(code),
            ct);
    }

    public async Task<IReadOnlyList<App>> GetAllAsync(CancellationToken ct = default)
    {
        return await appRepo.GetAllAsync(ct);
    }

    public async Task<(IReadOnlyList<App> Items, int TotalCount)> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        return await appRepo.SearchAsync(search, page, pageSize, ct);
    }
}
