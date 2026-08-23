using NovaCore.BuildingBlock.Application.Abstractions.Jobs;

namespace NovaCore.Content.Infrastructure.BackgroundJobs.Jobs.HardDeleteContent;

public sealed class HardDeleteContentJobOptions : IJobOptions
{
    public const string Section = "Jobs:HardDeleteContent";

    public string JobId { get; set; } = "content-hard-delete";
    public string CronExpression { get; set; } = "0 * * * *";
    public string Queue { get; set; } = "default";
    public bool IsInit { get; set; }

    /// <summary>Business default is 7 days (WCM spec section 12.3) - configurable per environment,
    /// never intended to ship lower in production.</summary>
    public int RetentionDays { get; set; } = 7;

    /// <summary>How many eligible Content rows are permanently removed - and committed - per
    /// transaction, matching RefreshTokenSyncService's batching shape for the same reason: a very
    /// large backlog shouldn't hold one open transaction indefinitely.</summary>
    public int BatchSize { get; set; } = 100;
}
