using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Entities.Apps;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Accounts.Write;

public sealed class AccountAppAssignmentService(
    AuthDbContext dbContext,
    IUnitOfWork unitOfWork) : IAccountAppAssignmentService, IPersistenceService
{
    public async Task AssignAsync(Guid accountId, Guid appId, CancellationToken ct = default)
    {
        var account = await dbContext.Users
            .Include(a => a.AccountApps)
            .FirstOrDefaultAsync(a => a.Id == accountId, ct)
            ?? throw ExceptionFactory.EntityNotFound<Account>(accountId);

        if (account.AccountApps.Any(aa => aa.AppId == appId))
            return;

        var app = await dbContext.Apps.FirstOrDefaultAsync(a => a.Id == appId, ct)
            ?? throw ExceptionFactory.EntityNotFound<App>(appId);

        account.AssignApp(app);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(Guid accountId, Guid appId, CancellationToken ct = default)
    {
        var account = await dbContext.Users
            .Include(a => a.AccountApps)
            .FirstOrDefaultAsync(a => a.Id == accountId, ct)
            ?? throw ExceptionFactory.EntityNotFound<Account>(accountId);

        if (!account.AccountApps.Any(aa => aa.AppId == appId))
            return;

        account.RemoveApp(appId);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<bool> IsAssignedAsync(Guid accountId, Guid appId, CancellationToken ct = default)
    {
        return await dbContext.AccountApps
            .AsNoTracking()
            .AnyAsync(aa => aa.AccountId == accountId && aa.AppId == appId, ct);
    }
}
