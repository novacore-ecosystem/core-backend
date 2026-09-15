using System.Security.Cryptography;

using NovaCore.Auth.Application.Abstractions.Security.PasswordReset;

using NovaCore.BuildingBlock.Application.Abstractions.Services;

namespace NovaCore.Auth.Infrastructure.Security.PasswordReset;

/// <summary>
/// Redis-backed, single-use reset token store - the same DI/config shape as
/// RefreshTokenCacheService, minus the sync-to-Postgres concern (a reset token never needs to
/// survive a Redis flush the way a refresh token does, so it's Redis-only with no Postgres entity).
/// </summary>
public sealed class PasswordResetTokenService(ICacheService cacheService) : IPasswordResetTokenService, IAppService
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(CacheKeyConstant.PasswordResetTokens.DefaultTtlMinutes);

    public async Task<string> GenerateTokenAsync(Guid accountId, CancellationToken ct = default)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        await cacheService.SetAsync(CacheKeyConstant.PasswordResetTokens.ByTokenString(token), accountId, TokenLifetime, ct);
        return token;
    }

    public async Task<(Guid AccountId, bool IsValid)> ValidateTokenAsync(string token, CancellationToken ct = default)
    {
        var accountId = await cacheService.GetAsync<Guid?>(CacheKeyConstant.PasswordResetTokens.ByTokenString(token), ct);
        return accountId is null ? (Guid.Empty, false) : (accountId.Value, true);
    }

    public Task InvalidateTokenAsync(string token, CancellationToken ct = default)
        => cacheService.RemoveAsync(CacheKeyConstant.PasswordResetTokens.ByTokenString(token), ct);
}
