using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Enums;
using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.Sessions.Repositories;

public interface ISessionRepository : IRepository<Session>
{
    /// <summary>Every Active Session for one Account, tracked (not AsNoTracking) - the one caller
    /// (RevokeAllActiveByAccountIdAsync) mutates each row via Session.Revoke before its own
    /// caller's SaveChanges/ExecuteTransactionAsync commits.</summary>
    Task<IReadOnlyList<Session>> ListActiveByAccountIdAsync(Guid accountId, CancellationToken ct = default);

    /// <summary>Revokes every currently-Active Session for one Account. Non-committing - the
    /// caller's own unit of work commits the change.</summary>
    Task RevokeAllActiveByAccountIdAsync(Guid accountId, RevocationReason reason, CancellationToken ct = default);
}
