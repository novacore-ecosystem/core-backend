using NovaCore.Auth.Application.Abstractions.Persistence.RefreshTokens;
using NovaCore.Auth.Domain.Entities.Accounts;

using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.RefreshTokens.Write;

/// <summary>
/// Both methods are non-committing - the one caller (RefreshTokenSyncService) batches many
/// Add/Update calls across a whole user-batch into one IUnitOfWork.ExecuteTransactionAsync it
/// owns itself. See the persistence refactor tracker's extension of Correction 2 to Auth.
/// </summary>
public sealed class RefreshTokenWriteService(
    IRepository<RefreshToken, Guid> repo) : IRefreshTokenWriteService
{
    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        await repo.AddAsync(token, ct);
    }

    public async Task UpdateAsync(Guid id, Func<RefreshToken, Task> updateAction, CancellationToken ct = default)
    {
        await repo.UpdateAsync(id, updateAction, ct);
    }
}
