using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Application.Abstractions.Persistence.Apps;
using NovaCore.Auth.Domain.Entities.Apps;
using NovaCore.Auth.Persistence.Contexts.Apps.Repositories;

using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Apps.Write;

public sealed class AppWriteService(
    IAppRepository repo,
    IUnitOfWork unitOfWork) : IAppWriteService, IPersistenceService
{
    public async Task CreateAsync(App app, CancellationToken ct = default)
    {
        await repo.AddAsync(app, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Guid id, Action<App> update, CancellationToken ct = default)
    {
        await repo.UpdateAsync(a => a.Id == id, update, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateWithTranslationsAsync(Guid id, Action<App> update, CancellationToken ct = default)
    {
        await repo.UpdateAsync(a => a.Id == id, q => q.Include(a => a.Translations), update, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await repo.DeleteAsync(a => a.Id == id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
