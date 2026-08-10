using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.BuildingBlock.Application.Abstractions.Idempotency;

/// <summary>Configuration for the Idempotency + Distributed Lock framework. Bound from config section "Idempotency".</summary>
public sealed class IdempotencyOptions
{
    public const string Section = "Idempotency";

    /// <summary>Request header carrying the client-supplied idempotency key (default: "Idempotency-Key")</summary>
    public string HeaderName { get; set; } = HeaderKeyConstant.IdempotencyKey;

    /// <summary>Max time to wait to acquire the distributed lock before returning 409 (default: 10s)</summary>
    public int LockTimeoutSeconds { get; set; } = 10;

    /// <summary>Safety-net TTL on the lock itself, in case the holder crashes without releasing (default: 30s)</summary>
    public int LockExpirationSeconds { get; set; } = 30;

    /// <summary>How long a cached response stays replayable for duplicate requests (default: 1440 = 24h)</summary>
    public int RecordExpirationMinutes { get; set; } = 1440;

    /// <summary>Responses larger than this are never cached - the request still succeeds, it just won't be idempotency-replayable (default: 64 KB)</summary>
    public int MaxCachedResponseSizeBytes { get; set; } = 65536;
}
