using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Enums;
using NovaCore.Auth.Persistence.Contexts;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.Sessions.Repositories;

public sealed class SessionRepository(AuthDbContext dbContext)
    : AuthBaseRepository<Session>(dbContext), ISessionRepository
{
    public async Task<IReadOnlyList<Session>> ListActiveByAccountIdAsync(Guid accountId, CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.AccountId == accountId && s.Status == SessionStatus.Active)
            .ToListAsync(ct);
    }

    public async Task RevokeAllActiveByAccountIdAsync(Guid accountId, RevocationReason reason, CancellationToken ct = default)
    {
        var sessions = await _dbSet
            .Where(s => s.AccountId == accountId && s.Status == SessionStatus.Active)
            .ToListAsync(ct);

        foreach (var session in sessions)
            session.Revoke(reason);
    }
}
