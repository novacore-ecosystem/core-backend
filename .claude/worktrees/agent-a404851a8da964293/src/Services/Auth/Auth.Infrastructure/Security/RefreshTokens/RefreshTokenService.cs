using NovaCore.Auth.Application.Abstractions.Security.Jwt;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Infrastructure.Caching;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.SharedKernel.Security;

namespace NovaCore.Auth.Infrastructure.Security.RefreshTokens;

public sealed class RefreshTokenService(
    IJwtTokenGenerator jwtTokenGenerator,
    JwtSettings jwtSettings,
    RefreshTokenCacheService cacheService) : IRefreshTokenService, IAppService
{
    public async Task<string> GenerateRefreshTokenAsync(Guid userId, Guid jwtId, CancellationToken ct = default)
    {
        var token = jwtTokenGenerator.GenerateRefreshToken();
        var expiryDate = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenExpirationDays);

        var refreshToken = RefreshToken.Create(jwtId, token, expiryDate, userId);

        await cacheService.SetAsync(refreshToken, ct: ct);

        return token;
    }

    public async Task<bool> ValidateRefreshTokenAsync(Guid userId, string token, CancellationToken ct = default)
    {
        var cachedToken = await cacheService.GetByTokenStringAsync(token, ct);
        return cachedToken != null &&
               cachedToken.UserId == userId &&
               !cachedToken.IsRevoked &&
               cachedToken.ExpiryDate > DateTime.UtcNow;
    }

    public async Task<(Guid UserId, bool IsValid)> ValidateAndGetUserIdAsync(string token, CancellationToken ct = default)
    {
        var cachedToken = await cacheService.GetByTokenStringAsync(token, ct);
        if (cachedToken == null)
            return (Guid.Empty, false);

        var isValid = !cachedToken.IsRevoked && cachedToken.ExpiryDate > DateTime.UtcNow;
        return (cachedToken.UserId, isValid);
    }

    public async Task RevokeRefreshTokenByTokenStringAsync(string token, CancellationToken ct = default)
    {
        await cacheService.RevokeByTokenStringAsync(token, ct);
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken ct = default)
    {
        await cacheService.RevokeAllForUserAsync(userId, ct);
    }

    public async Task CleanupExpiredTokensAsync(CancellationToken ct = default)
    {
        await Task.CompletedTask;
    }
}
