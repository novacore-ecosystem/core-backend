namespace NovaCore.Auth.Application.Abstractions.Services;

public interface IRefreshTokenService
{
    Task<string> GenerateRefreshTokenAsync(Guid userId, Guid jwtId, CancellationToken ct = default);

    Task<bool> ValidateRefreshTokenAsync(Guid userId, string token, CancellationToken ct = default);

    Task<(Guid UserId, bool IsValid)> ValidateAndGetUserIdAsync(string token, CancellationToken ct = default);

    Task RevokeRefreshTokenByTokenStringAsync(string token, CancellationToken ct = default);

    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken ct = default);

    Task CleanupExpiredTokensAsync(CancellationToken ct = default);
}
