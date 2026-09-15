namespace NovaCore.Auth.Application.Abstractions.Persistence.Sessions;

public interface ISessionReadService
{
    Task<IReadOnlyList<Session>> GetActiveByAccountIdAsync(Guid accountId, CancellationToken ct = default);
}
