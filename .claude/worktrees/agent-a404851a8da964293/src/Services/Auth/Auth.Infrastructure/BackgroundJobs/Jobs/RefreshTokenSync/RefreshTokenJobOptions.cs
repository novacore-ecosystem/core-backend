using NovaCore.BuildingBlock.Application.Abstractions.Jobs;

namespace NovaCore.Auth.Infrastructure.BackgroundJobs.Jobs.RefreshTokenSync;

public sealed class RefreshTokenJobOptions : IJobOptions
{
    public const string Section = "Jobs:RefreshTokenSync";

    public string JobId { get; set; } = "refresh-token-sync";
    public string CronExpression { get; set; } = "*/5 * * * *";
    public string Queue { get; set; } = "default";
    public bool IsInit { get; set; } = true;

    /// <summary>How many active users are processed - and committed to the DB - per transaction</summary>
    public int UserBatchSize { get; set; } = 100;

    /// <summary>Max tokens per MGET call when fetching full cached payloads</summary>
    public int TokenBatchSize { get; set; } = 500;
}
