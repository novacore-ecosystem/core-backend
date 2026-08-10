namespace NovaCore.Auth.Application.Abstractions.Persistence.RefreshTokens;

public interface IRefreshTokenWriteService
{
    /// <summary>
    /// Non-committing: stages the add on the DbContext's change tracker only. The one caller
    /// (RefreshTokenSyncService) batches many Add/Update calls across a whole user-batch into one
    /// IUnitOfWork.ExecuteTransactionAsync it owns itself - see the persistence refactor tracker's
    /// extension of Correction 2 to Auth.
    /// </summary>
    Task AddAsync(RefreshToken token, CancellationToken ct = default);

    /// <summary>Non-committing, same batching reason as AddAsync.</summary>
    Task UpdateAsync(Guid id, Func<RefreshToken, Task> updateAction, CancellationToken ct = default);
}
