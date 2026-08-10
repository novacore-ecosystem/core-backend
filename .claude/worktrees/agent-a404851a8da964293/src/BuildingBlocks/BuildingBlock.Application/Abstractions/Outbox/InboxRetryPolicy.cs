namespace NovaCore.BuildingBlock.Application.Abstractions.Outbox;

/// <summary>
/// Retry/backoff policy, passed in by the caller (owned by NovaCore.BuildingBlock.Infrastructure's
/// InboxRetryOptions) rather than read from configuration here - this project stays
/// infrastructure-agnostic.
/// </summary>
public sealed record InboxRetryPolicy(
    int MaxRetryCount,
    TimeSpan InitialRetryDelay,
    double RetryBackoffMultiplier,
    TimeSpan MaximumRetryDelay);
