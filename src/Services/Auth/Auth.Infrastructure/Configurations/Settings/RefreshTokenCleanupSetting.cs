using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs;

namespace NovaCore.Auth.Infrastructure.Configurations.Settings;

/// <summary>
/// Hangfire schedule and batching knobs for the refresh-token cache cleanup job - an independent
/// backstop pass that removes only already-expired cache entries (see RefreshTokenCleanupJob's own
/// remarks for why it never touches a still-valid token), on its own looser cadence from
/// RefreshTokenSyncService's inline pruning.
/// </summary>
public sealed class RefreshTokenCleanupSetting : SchedulerSettingBase
{
    public const string Section = "Jobs:RefreshTokenCleanup";

    public override string JobId { get; set; } = "refresh-token-cleanup";
    public override string CronExpression { get; set; } = "*/30 * * * *";

    /// <summary>How many active users' per-user token index is scanned per batch</summary>
    public int UserBatchSize { get; set; } = 100;

    /// <summary>Max cache keys removed per batch-delete round trip</summary>
    public int DeleteBatchSize { get; set; } = 500;
}
