using NovaCore.Auth.Infrastructure.Caching;
using NovaCore.Auth.Infrastructure.Configurations.Settings;

using NovaCore.BuildingBlock.Application.Abstractions.Jobs;

using Microsoft.Extensions.Options;

namespace NovaCore.Auth.Infrastructure.BackgroundJobs.Jobs.RefreshTokenCleanup;

/// <summary>
/// Independent backstop pass that removes already-expired refresh-token cache entries, on its own
/// 30-minute cadence, in addition to (not instead of) RefreshTokenSyncService's own inline pruning
/// of expired/revoked entries during each sync cycle.
/// </summary>
/// <remarks>
/// Deliberately narrow in what it removes: a valid, unexpired refresh token stays in cache no
/// matter how long ago it was synced, because <c>RefreshTokenService.ValidateAndGetUserIdAsync</c>
/// reads only from cache with no database fallback - Redis is the live store for active tokens,
/// not a write-buffer, and <c>RefreshTokenInitializationService</c> exists specifically to
/// rehydrate it from Postgres at startup. Evicting a still-active token here would fail every
/// later refresh attempt for that session. So eligibility is a pure, unconditional time fact
/// (<c>ExpiryDate &lt;= now</c>, mirroring the exact same check SyncUserAsync's own expired-branch
/// already uses) - never a "this was Synced a while ago" check, since Redis's own per-key TTL
/// already reclaims the full <c>refresh_token:{token}</c> entry on expiry; what this job actually
/// cleans up is the per-user index (<c>user_refresh_tokens:{userId}</c>) hash fields, which don't
/// carry an individual TTL and would otherwise accumulate indefinitely, plus any full entry that
/// a crashed/interrupted sync cycle left behind.
/// </remarks>
public sealed class RefreshTokenCleanupJob(
    RefreshTokenCacheService cacheService,
    IAppLogger<RefreshTokenCleanupJob> logger,
    IOptions<RefreshTokenCleanupSetting> options) : IRecurringJob
{
    public string JobId => options.Value.JobId;
    public string CronExpression => options.Value.CronExpression;
    public string Queue => options.Value.Queue;
    public bool IsInit => options.Value.IsInit;

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
        => CleanupAsync(cancellationToken);

    public async Task CleanupAsync(CancellationToken ct = default)
    {
        logger.Information("Starting refresh token cache cleanup");
        var startedAt = DateTime.UtcNow;

        var userIds = await cacheService.GetActiveUserIdsAsync(ct);
        if (userIds.Count == 0)
        {
            logger.Debug("No active users to clean up");
            return;
        }

        var stats = new CleanupStats();
        var userBatchSize = Math.Max(1, options.Value.UserBatchSize);

        for (var i = 0; i < userIds.Count; i += userBatchSize)
        {
            var userBatch = userIds.Skip(i).Take(userBatchSize).ToList();
            foreach (var userId in userBatch)
                await CleanupUserAsync(userId, stats, ct);
        }

        logger.Information(
            "Refresh token cache cleanup completed. Scanned: {Scanned}, Eligible: {Eligible}, Deleted: {Deleted}, Failed: {Failed}, Duration: {DurationMs}ms",
            stats.Scanned, stats.Eligible, stats.Deleted, stats.Failed, (DateTime.UtcNow - startedAt).TotalMilliseconds);
    }

    private async Task CleanupUserAsync(Guid userId, CleanupStats stats, CancellationToken ct)
    {
        var index = await cacheService.GetUserTokenIndexAsync(userId, ct);
        if (index.Count == 0)
        {
            await cacheService.RemoveActiveUserAsync(userId, ct);
            return;
        }

        stats.Scanned += index.Count;

        var now = DateTime.UtcNow;
        var eligibleTokens = index
            .Where(kv => kv.Value is null || kv.Value.ExpiryDate <= now)
            .Select(kv => kv.Key)
            .ToList();

        if (eligibleTokens.Count > 0)
        {
            stats.Eligible += eligibleTokens.Count;

            var deleteBatchSize = Math.Max(1, options.Value.DeleteBatchSize);
            foreach (var chunk in eligibleTokens.Chunk(deleteBatchSize))
            {
                try
                {
                    await cacheService.RemoveManyAsync(userId, chunk, ct);
                    stats.Deleted += chunk.Length;
                }
                catch (Exception ex)
                {
                    stats.Failed += chunk.Length;
                    logger.Error(ex,
                        "Failed to delete a batch of {Count} expired refresh token cache entries for user {UserId}",
                        chunk.Length, userId);
                }
            }
        }

        // Live check, not derived from the counts above - a token generated concurrently while
        // this method ran must not be lost by dropping its owner from active_users.
        if (await cacheService.GetUserTokenCountAsync(userId, ct) == 0)
            await cacheService.RemoveActiveUserAsync(userId, ct);
    }

    private sealed class CleanupStats
    {
        public int Scanned;
        public int Eligible;
        public int Deleted;
        public int Failed;
    }
}
