using NovaCore.BuildingBlock.Application.Abstractions.Outbox;

namespace NovaCore.BuildingBlock.Infrastructure.Messaging;

public sealed class InboxRetryOptions
{
    public const string Section = "Inbox:Retry";

    /// <summary>How often InboxRetryHostedService polls for due Retrying rows.</summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>Rows fetched per poll.</summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>Attempts (including the first) before a row moves to DeadLetter.</summary>
    public int MaxRetryCount { get; set; } = 5;

    /// <summary>Delay before the first retry.</summary>
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>Multiplier applied to the delay after each further failed attempt (exponential backoff).</summary>
    public double RetryBackoffMultiplier { get; set; } = 2.0;

    /// <summary>Upper bound on the computed delay, regardless of RetryCount.</summary>
    public TimeSpan MaximumRetryDelay { get; set; } = TimeSpan.FromMinutes(30);

    public InboxRetryPolicy ToPolicy() => new(MaxRetryCount, InitialRetryDelay, RetryBackoffMultiplier, MaximumRetryDelay);
}
