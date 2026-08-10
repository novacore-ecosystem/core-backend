using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Persistence.Contexts.Accounts.Repositories;

namespace NovaCore.Auth.Persistence.Contexts.Accounts.Write;

/// <summary>
/// Non-committing - OnAccountDeletionInitiatedHandler owns IUnitOfWork.ExecuteTransactionAsync
/// itself, matching its original (pre-migration) commit shape.
/// </summary>
public sealed class AccountWriteService(
    IAccountRepository repo) : IAccountWriteService
{
    public async Task DeleteIfExistAsync(Guid id, CancellationToken ct = default)
    {
        await repo.DeleteIfExistAsync(id, ct);
    }
}
