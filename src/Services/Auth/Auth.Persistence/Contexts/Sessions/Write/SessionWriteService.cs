using NovaCore.Auth.Application.Abstractions.Persistence.Sessions;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Enums;
using NovaCore.Auth.Domain.ValueObjects;
using NovaCore.Auth.Persistence.Contexts.Sessions.Repositories;

using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Sessions.Write;

/// <summary>Both methods are non-committing - callers (OnAuthenticationSucceededHandler,
/// ResetPassword-family handlers) own the surrounding IUnitOfWork.ExecuteTransactionAsync themselves.</summary>
public sealed class SessionWriteService(ISessionRepository sessionRepo)
    : ISessionWriteService, IPersistenceService
{
    public async Task CreateAsync(Guid accountId, IpAddress ipAddress, DateTime expiresAt, CancellationToken ct = default)
    {
        var session = Session.Create(accountId, ipAddress, expiresAt);
        await sessionRepo.AddAsync(session, ct);
    }

    public async Task RevokeAllActiveByAccountIdAsync(Guid accountId, RevocationReason reason, CancellationToken ct = default)
    {
        await sessionRepo.RevokeAllActiveByAccountIdAsync(accountId, reason, ct);
    }
}
