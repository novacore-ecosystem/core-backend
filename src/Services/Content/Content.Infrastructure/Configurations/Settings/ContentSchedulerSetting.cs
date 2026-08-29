using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs;

namespace NovaCore.Content.Infrastructure.Configurations.Settings;

/// <summary>Hangfire schedule and batching knobs for the soft-deleted Content hard-delete retention job.</summary>
public sealed class ContentSchedulerSetting : SchedulerSettingBase
{
    public const string Section = "Jobs:HardDeleteContent";

    public override string JobId { get; set; } = "content-hard-delete";
    public override string CronExpression { get; set; } = "0 * * * *";
    public override bool IsInit { get; set; } = false;

    /// <summary>Business default is 7 days (WCM spec section 12.3) - configurable per environment,
    /// never intended to ship lower in production.</summary>
    public int RetentionDays { get; set; } = 7;

    /// <summary>How many eligible Content rows are permanently removed - and committed - per
    /// transaction, matching RefreshTokenSyncService's batching shape for the same reason: a very
    /// large backlog shouldn't hold one open transaction indefinitely.</summary>
    public int BatchSize { get; set; } = 100;
}
