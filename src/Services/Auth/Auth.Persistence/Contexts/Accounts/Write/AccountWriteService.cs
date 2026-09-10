using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Persistence.Contexts.Accounts.Repositories;

using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Accounts.Write;

public sealed class AccountWriteService(
    IAccountRepository repo,
    IUnitOfWork unitOfWork) : IAccountWriteService
{
    /// <summary>Non-committing - OnAccountDeletionInitiatedHandler owns
    /// IUnitOfWork.ExecuteTransactionAsync itself, matching its original (pre-migration) commit
    /// shape.</summary>
    public async Task DeleteIfExistAsync(Guid id, CancellationToken ct = default)
    {
        await repo.DeleteIfExistAsync(id, ct);
    }

    public async Task SetLevelAsync(Guid id, int level, CancellationToken ct = default)
    {
        await repo.UpdateAsync(a => a.Id == id, a => a.SetLevel(level), ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
