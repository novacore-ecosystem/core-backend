
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.Infrastructure.Caching;

public sealed class RefreshTokenCacheService(ICacheService cacheService)
{
    public async Task SetAsync(
        RefreshToken token,
        TokenSyncStatus status = TokenSyncStatus.New,
        CancellationToken ct = default)
    {
        var expiration = token.ExpiryDate - DateTime.UtcNow;
        if (expiration <= TimeSpan.Zero)
            return;

        var now = DateTime.UtcNow;
        var tokenKey = CacheKeyConstant.RefreshTokens.ByTokenString(token.Token);
        var cacheEntry = new CachedRefreshToken
        {
            Id = token.Id,
            JwtId = token.JwtId,
            UserId = token.AccountId,
            ExpiryDate = token.ExpiryDate,
            IsRevoked = token.IsRevoked,
            CachedAt = now,
            SyncStatus = status
        };

        await cacheService.SetAsync(tokenKey, cacheEntry, expiration, ct);

        var userKey = CacheKeyConstant.RefreshTokens.UserTokens(token.AccountId);
        await cacheService.HashSetAsync(userKey, token.Token, new UserRefreshTokenIndex
        {
            SyncStatus = status,
            CachedAt = now,
            ExpiryDate = token.ExpiryDate
        }, ct);

        await cacheService.SetAddAsync(CacheKeyConstant.RefreshTokens.ActiveUsers, token.AccountId.ToString(), ct);
    }

    public Task<CachedRefreshToken?> GetByTokenStringAsync(string token, CancellationToken ct = default)
        => cacheService.GetAsync<CachedRefreshToken>(CacheKeyConstant.RefreshTokens.ByTokenString(token), ct);

    public async Task RevokeByTokenStringAsync(string token, CancellationToken ct = default)
    {
        var cached = await GetByTokenStringAsync(token, ct);
        if (cached is null)
            return;

        await RevokeCachedAsync(token, cached, ct);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var index = await GetUserTokenIndexAsync(userId, ct);
        if (index.Count == 0)
            return;

        var cachedTokens = await GetManyByTokenStringAsync(index.Keys, ct);
        foreach (var (token, cached) in cachedTokens)
        {
            if (cached is null || cached.IsRevoked)
                continue;

            await RevokeCachedAsync(token, cached, ct);
        }
    }

    /// <summary>
    /// Revokes a cached token. Tokens that never made it to the DB (still New) are dropped outright -
    /// there's nothing to persist, and it saves the sync job a doomed UPDATE against a row that
    /// doesn't exist yet. Tokens the DB already knows about are flagged Revoked for the sync job to persist.
    /// </summary>
    private Task RevokeCachedAsync(string token, CachedRefreshToken cached, CancellationToken ct)
    {
        if (cached.SyncStatus == TokenSyncStatus.New)
            return RemoveAsync(cached.UserId, token, ct);

        cached.IsRevoked = true;
        cached.SyncStatus = TokenSyncStatus.Revoked;
        return WriteBackAsync(token, cached, ct);
    }

    /// <summary>All user IDs that currently own at least one active refresh token (SMEMBERS, single round trip).</summary>
    public async Task<IReadOnlyList<Guid>> GetActiveUserIdsAsync(CancellationToken ct = default)
    {
        var members = await cacheService.SetMembersAsync(CacheKeyConstant.RefreshTokens.ActiveUsers, ct);
        var userIds = new List<Guid>(members.Count);
        foreach (var member in members)
        {
            if (Guid.TryParse(member, out var userId))
                userIds.Add(userId);
        }
        return userIds;
    }

    /// <summary>Lightweight per-user token index (HGETALL, single round trip).</summary>
    public Task<IDictionary<string, UserRefreshTokenIndex?>> GetUserTokenIndexAsync(Guid userId, CancellationToken ct = default)
        => cacheService.HashGetAllAsync<UserRefreshTokenIndex>(CacheKeyConstant.RefreshTokens.UserTokens(userId), ct);

    /// <summary>Bulk fetch of full cached token payloads via MGET (single round trip per batch).</summary>
    public async Task<IDictionary<string, CachedRefreshToken?>> GetManyByTokenStringAsync(
        IEnumerable<string> tokens,
        CancellationToken ct = default)
    {
        var tokensList = tokens.ToList();
        if (tokensList.Count == 0)
            return new Dictionary<string, CachedRefreshToken?>();

        var keyed = await cacheService.GetManyAsync<CachedRefreshToken>(
            tokensList.Select(CacheKeyConstant.RefreshTokens.ByTokenString), ct);

        var result = new Dictionary<string, CachedRefreshToken?>(tokensList.Count);
        foreach (var token in tokensList)
            result[token] = keyed.TryGetValue(CacheKeyConstant.RefreshTokens.ByTokenString(token), out var v) ? v : null;

        return result;
    }

    /// <summary>Flags a token as Synced in both the full entry and the user index, without re-syncing it next run.</summary>
    public Task MarkSyncedAsync(string token, CachedRefreshToken cached, CancellationToken ct = default)
    {
        cached.SyncStatus = TokenSyncStatus.Synced;
        return WriteBackAsync(token, cached, ct);
    }

    /// <summary>Hard-removes a token from cache: the full entry and its user-index field. Idempotent.</summary>
    public async Task RemoveAsync(Guid userId, string token, CancellationToken ct = default)
    {
        await cacheService.RemoveAsync(CacheKeyConstant.RefreshTokens.ByTokenString(token), ct);
        await cacheService.HashDeleteAsync(CacheKeyConstant.RefreshTokens.UserTokens(userId), token, ct);
    }

    /// <summary>Drops a user from the active-users set once they own no more tokens.</summary>
    public Task RemoveActiveUserAsync(Guid userId, CancellationToken ct = default)
        => cacheService.SetRemoveAsync(CacheKeyConstant.RefreshTokens.ActiveUsers, userId.ToString(), ct);

    /// <summary>
    /// Live HLEN check (not derived from a snapshot) - used right before dropping a user from
    /// active_users so a token added mid-sync-run isn't lost by acting on stale counts.
    /// </summary>
    public Task<long> GetUserTokenCountAsync(Guid userId, CancellationToken ct = default)
        => cacheService.HashLengthAsync(CacheKeyConstant.RefreshTokens.UserTokens(userId), ct);

    private async Task WriteBackAsync(string token, CachedRefreshToken cached, CancellationToken ct)
    {
        var expiration = cached.ExpiryDate - DateTime.UtcNow;
        if (expiration <= TimeSpan.Zero)
        {
            await RemoveAsync(cached.UserId, token, ct);
            return;
        }

        await cacheService.SetAsync(CacheKeyConstant.RefreshTokens.ByTokenString(token), cached, expiration, ct);
        await cacheService.HashSetAsync(CacheKeyConstant.RefreshTokens.UserTokens(cached.UserId), token, new UserRefreshTokenIndex
        {
            SyncStatus = cached.SyncStatus,
            CachedAt = cached.CachedAt,
            ExpiryDate = cached.ExpiryDate
        }, ct);
    }

    /// <summary>
    /// Cached refresh token payload. The raw token string is intentionally not duplicated here -
    /// it's already the Redis key (refresh_token:{token}) and the user-index hash field, so every
    /// caller that has a CachedRefreshToken also has the token string that produced it.
    /// </summary>
    public sealed class CachedRefreshToken
    {
        public Guid Id { get; set; }
        public Guid JwtId { get; set; }
        public Guid UserId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CachedAt { get; set; }
        public TokenSyncStatus SyncStatus { get; set; } = TokenSyncStatus.New;
    }

    /// <summary>Lightweight per-user index entry - just enough for the sync job to triage without a full fetch.</summary>
    public sealed class UserRefreshTokenIndex
    {
        public TokenSyncStatus SyncStatus { get; set; }
        public DateTime CachedAt { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public enum TokenSyncStatus
    {
        New = 0,
        Modified = 1,
        Revoked = 2,
        Synced = 3
    }
}
