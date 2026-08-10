namespace NovaCore.Payment.Domain.Entities.Operations;

/// <summary>
/// Business-level, persisted dedup of gateway-facing requests (e.g. "have we already sent this
/// exact capture request to Stripe?"). Distinct from and complementary to the Redis-backed
/// transport-level IIdempotencyStore framework (NovaCore.BuildingBlock.Application.Abstractions.
/// Idempotency) already reused at the API layer for HTTP request dedup - that one is short-lived
/// and cache-only; this one is a durable record for gateway-call-level dedup/audit.
/// </summary>
public sealed class IdempotencyRecord : BaseEntity<Guid>
{
    public string Key { get; private set; } = string.Empty;
    public string RequestHash { get; private set; } = string.Empty;
    public string? ResponseSnapshot { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    private IdempotencyRecord() { }

    public static IdempotencyRecord Create(string key, string requestHash, DateTime expiresAt, string? responseSnapshot = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw ExceptionFactory.RequiredField("Idempotency key cannot be empty.");

        if (string.IsNullOrWhiteSpace(requestHash))
            throw ExceptionFactory.RequiredField("Idempotency request hash cannot be empty.");

        return new IdempotencyRecord
        {
            Id = Guid.CreateVersion7(),
            Key = key.Trim(),
            RequestHash = requestHash.Trim(),
            ResponseSnapshot = responseSnapshot,
            ExpiresAt = expiresAt,
        };
    }
}
