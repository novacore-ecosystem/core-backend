using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs;

namespace NovaCore.Auth.Infrastructure.Configurations.Settings;

/// <summary>Hangfire schedule and batching knobs for the refresh-token cache-to-Postgres sync job.</summary>
public sealed class AuthSchedulerSetting : SchedulerSettingBase
{
    public const string Section = "Jobs:RefreshTokenSync";

    public override string JobId { get; set; } = "refresh-token-sync";
    public override string CronExpression { get; set; } = "*/5 * * * *";

    /// <summary>How many active users are processed - and committed to the DB - per transaction</summary>
    public int UserBatchSize { get; set; } = 100;

    /// <summary>Max tokens per MGET call when fetching full cached payloads</summary>
    public int TokenBatchSize { get; set; } = 500;
}
