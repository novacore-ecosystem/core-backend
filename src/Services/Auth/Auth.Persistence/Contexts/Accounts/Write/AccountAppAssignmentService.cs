using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Domain.Entities.Apps;
using NovaCore.Auth.Persistence.Contexts.Accounts.Repositories;
using NovaCore.Auth.Persistence.Contexts.Apps.Repositories;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Accounts.Write;

public sealed class AccountAppAssignmentService(
    IAccountRepository accountRepo,
    IAppRepository appRepo,
    IUnitOfWork unitOfWork) : IAccountAppAssignmentService, IPersistenceService
{
    public async Task AssignAsync(Guid accountId, Guid appId, CancellationToken ct = default)
    {
        var hasChanges = false;
        await accountRepo.UpdateAsync(
            a => a.Id == accountId,
            q => q.Include(a => a.AccountApps),
            async account =>
            {
                if (account.AccountApps.Any(aa => aa.AppId == appId))
                    return;

                var app = await appRepo.GetAsync(a => a.Id == appId, ct)
                    ?? throw ExceptionFactory.EntityNotFound<App>(appId);

                account.AssignApp(app);
                hasChanges = true;
            },
            ct);

        if (hasChanges)
            await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(Guid accountId, Guid appId, CancellationToken ct = default)
    {
        var hasChanges = false;
        await accountRepo.UpdateAsync(
            a => a.Id == accountId,
            q => q.Include(a => a.AccountApps),
            account =>
            {
                if (!account.AccountApps.Any(aa => aa.AppId == appId))
                    return;

                account.RemoveApp(appId);
                hasChanges = true;
            },
            ct);

        if (hasChanges)
            await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<bool> IsAssignedAsync(Guid accountId, Guid appId, CancellationToken ct = default)
    {
        return await accountRepo.IsAssignedToAppAsync(accountId, appId, ct);
    }

    public async Task<IReadOnlyCollection<Guid>> GetAccountIdsByAppAsync(Guid appId, CancellationToken ct = default)
    {
        return await appRepo.GetAccountIdsAsync(appId, ct);
    }
}
