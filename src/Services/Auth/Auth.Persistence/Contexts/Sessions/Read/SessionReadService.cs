using NovaCore.Auth.Application.Abstractions.Persistence.Sessions;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Persistence.Contexts.Sessions.Repositories;

using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Sessions.Read;

public sealed class SessionReadService(ISessionRepository sessionRepo)
    : ISessionReadService, IPersistenceService
{
    public async Task<IReadOnlyList<Session>> GetActiveByAccountIdAsync(Guid accountId, CancellationToken ct = default)
    {
        return await sessionRepo.ListActiveByAccountIdAsync(accountId, ct);
    }
}
