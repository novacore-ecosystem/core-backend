namespace NovaCore.Auth.Application.Abstractions.Persistence.RefreshTokens;

public interface IRefreshTokenReadService
{
    Task<List<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
