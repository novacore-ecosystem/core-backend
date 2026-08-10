using NovaCore.Auth.Application.Abstractions.Persistence.RefreshTokens;
using NovaCore.Auth.Domain.Enums;
using NovaCore.Auth.Infrastructure.Caching;

using NovaCore.BuildingBlock.Application.Abstractions.Jobs;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

using Microsoft.Extensions.Options;

namespace NovaCore.Auth.Infrastructure.BackgroundJobs.Jobs.RefreshTokenSync;

/// <summary>
/// Flushes cached refresh token state to Postgres. Discovery is scan-free end to end:
/// SMEMBERS active_users -> HGETALL user_refresh_tokens:{userId} (cheap index, used to skip
/// tokens that are already Synced or already expired without fetching them) -> MGET the
/// remaining refresh_token:{token} payloads in configurable batches.
/// </summary>
public sealed class RefreshTokenSyncService(
    IRefreshTokenWriteService refreshTokenWriteService,
    RefreshTokenCacheService cacheService,
    IUnitOfWork unitOfWork,
    IAppLogger<RefreshTokenSyncService> logger,
    IOptions<RefreshTokenJobOptions> options) : IRecurringJob
{
    public string JobId => options.Value.JobId;
    public string CronExpression => options.Value.CronExpression;
    public string Queue => options.Value.Queue;
    public bool IsInit => options.Value.IsInit;

    private const int MaxRetries = 3;
    private const int InitialBackoffMs = 100;

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
        => SyncCacheToDbAsync(cancellationToken);

    public async Task SyncCacheToDbAsync(CancellationToken ct = default)
    {
        logger.Information("Starting refresh token cache synchronization");

        var userIds = await cacheService.GetActiveUserIdsAsync(ct);
        if (userIds.Count == 0)
        {
            logger.Debug("No active users to synchronize");
            return;
        }

        var stats = new SyncStats();
        var userBatchSize = Math.Max(1, options.Value.UserBatchSize);

        for (var i = 0; i < userIds.Count; i += userBatchSize)
        {
            var userBatch = userIds.Skip(i).Take(userBatchSize).ToList();
            await ExecuteWithRetryAsync(() => SyncUserBatchAsync(userBatch, stats, ct), ct);
        }

        logger.Information(
            "Token synchronization completed. Created: {Created}, Updated: {Updated}, Revoked: {Revoked}, Expired: {Expired}, UsersCleared: {UsersCleared}",
            stats.Created, stats.Updated, stats.Revoked, stats.Expired, stats.UsersCleared);
    }

    private async Task ExecuteWithRetryAsync(Func<Task> operation, CancellationToken ct)
    {
        var retryCount = 0;

        while (true)
        {
            try
            {
                await operation();
                return;
            }
            catch (Exception ex) when (IsDeadlock(ex) && retryCount < MaxRetries)
            {
                retryCount++;
                var backoffMs = InitialBackoffMs * (int)Math.Pow(2, retryCount - 1);
                logger.Error(ex, "Deadlock detected. Retry {Retry}/{Max} after {BackoffMs}ms",
                    retryCount, MaxRetries, backoffMs);
                await Task.Delay(backoffMs, ct);
            }
        }
    }

    private static bool IsDeadlock(Exception ex)
    {
        var message = ex.Message.ToLowerInvariant();
        var innerMessage = ex.InnerException?.Message.ToLowerInvariant() ?? string.Empty;

        return message.Contains("deadlock")
            || innerMessage.Contains("deadlock")
            || message.Contains("40p01")
            || innerMessage.Contains("40p01");
    }

    private async Task SyncUserBatchAsync(List<Guid> userIds, SyncStats stats, CancellationToken ct)
    {
        await unitOfWork.ExecuteTransactionAsync(
            action: async () =>
            {
                foreach (var userId in userIds)
                    await SyncUserAsync(userId, stats, ct);
            },
            ct: ct);
    }

    private async Task SyncUserAsync(Guid userId, SyncStats stats, CancellationToken ct)
    {
        var index = await cacheService.GetUserTokenIndexAsync(userId, ct);
        if (index.Count == 0)
        {
            await cacheService.RemoveActiveUserAsync(userId, ct);
            return;
        }

        var now = DateTime.UtcNow;

        // Anything already expired can be dropped straight from the index - no need to fetch
        // its full payload just to find out it's expired.
        var expiredTokens = index.Where(kv => kv.Value is null || kv.Value.ExpiryDate <= now)
            .Select(kv => kv.Key)
            .ToList();
        foreach (var token in expiredTokens)
        {
            await cacheService.RemoveAsync(userId, token, ct);
            stats.Expired++;
        }

        // Anything already Synced needs no work at all this cycle.
        var pendingTokens = index
            .Where(kv => kv.Value is not null
                && kv.Value.ExpiryDate > now
                && kv.Value.SyncStatus != RefreshTokenCacheService.TokenSyncStatus.Synced)
            .Select(kv => kv.Key)
            .ToList();

        var tokenBatchSize = Math.Max(1, options.Value.TokenBatchSize);
        foreach (var chunk in pendingTokens.Chunk(tokenBatchSize))
        {
            var cachedTokens = await cacheService.GetManyByTokenStringAsync(chunk, ct);
            foreach (var (token, cached) in cachedTokens)
                await SyncTokenAsync(userId, token, cached, now, stats, ct);
        }

        // Live check, not derived from the counts above - a token generated concurrently while
        // this method ran must not be lost by dropping its owner from active_users.
        if (await cacheService.GetUserTokenCountAsync(userId, ct) == 0)
        {
            await cacheService.RemoveActiveUserAsync(userId, ct);
            stats.UsersCleared++;
        }
    }

    private async Task SyncTokenAsync(
        Guid userId,
        string token,
        RefreshTokenCacheService.CachedRefreshToken? cached,
        DateTime now,
        SyncStats stats,
        CancellationToken ct)
    {
        if (cached is null)
        {
            // Evicted (or race with a concurrent revoke) between the index read and the MGET.
            await cacheService.RemoveAsync(userId, token, ct);
            return;
        }

        if (cached.ExpiryDate <= now)
        {
            await cacheService.RemoveAsync(userId, token, ct);
            stats.Expired++;
            return;
        }

        switch (cached.SyncStatus)
        {
            case RefreshTokenCacheService.TokenSyncStatus.New:
                if (await TryPersistAsync(() => refreshTokenWriteService.AddAsync(
                        RefreshToken.Create(cached.JwtId, token, cached.ExpiryDate, cached.UserId), ct),
                    cached.JwtId, "create"))
                {
                    await cacheService.MarkSyncedAsync(token, cached, ct);
                    stats.Created++;
                }
                break;

            case RefreshTokenCacheService.TokenSyncStatus.Modified:
                if (await TryPersistAsync(() => refreshTokenWriteService.UpdateAsync(cached.Id, rt =>
                    {
                        if (cached.IsRevoked && !rt.IsRevoked)
                            rt.Revoke(RevocationReason.Superseded);
                        return Task.CompletedTask;
                    }, ct), cached.JwtId, "update"))
                {
                    await cacheService.MarkSyncedAsync(token, cached, ct);
                    stats.Updated++;
                }
                break;

            case RefreshTokenCacheService.TokenSyncStatus.Revoked:
                // Only reaches Revoked once it was already Synced/Modified in cache (see
                // RefreshTokenCacheService.RevokeCachedAsync), so the DB row is guaranteed to exist.
                if (await TryPersistAsync(() => refreshTokenWriteService.UpdateAsync(cached.Id, rt =>
                    {
                        if (!rt.IsRevoked)
                            rt.Revoke(RevocationReason.Superseded);
                        return Task.CompletedTask;
                    }, ct), cached.JwtId, "revoke"))
                {
                    stats.Revoked++;
                }
                await cacheService.RemoveAsync(userId, token, ct);
                break;
        }
    }

    private async Task<bool> TryPersistAsync(Func<Task> action, Guid jwtId, string operation)
    {
        try
        {
            await action();
            logger.Debug("Synced token {JwtId} ({Operation})", jwtId, operation);
            return true;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to {Operation} token {JwtId}", operation, jwtId);
            return false;
        }
    }

    private sealed class SyncStats
    {
        public int Created;
        public int Updated;
        public int Revoked;
        public int Expired;
        public int UsersCleared;
    }
}
