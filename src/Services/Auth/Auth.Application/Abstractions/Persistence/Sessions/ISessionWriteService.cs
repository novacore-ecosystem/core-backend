using NovaCore.Auth.Domain.Enums;
using NovaCore.Auth.Domain.ValueObjects;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Sessions;

public interface ISessionWriteService
{
    /// <summary>Non-committing - callers own the surrounding IUnitOfWork.ExecuteTransactionAsync/SaveChangesAsync.</summary>
    Task CreateAsync(Guid accountId, IpAddress ipAddress, DateTime expiresAt, CancellationToken ct = default);

    /// <summary>Non-committing, same reason as CreateAsync.</summary>
    Task RevokeAllActiveByAccountIdAsync(Guid accountId, RevocationReason reason, CancellationToken ct = default);
}
