namespace NovaCore.BuildingBlock.Application.Abstractions.Idempotency;

/// <summary>
/// Stores the outcome of a completed operation, keyed by an idempotency key, so a duplicate
/// request can be answered without re-running business logic. Has no locking responsibility of
/// its own - callers are expected to hold an <see cref="IDistributedLock"/> for the same key
/// while reading/writing here, otherwise two concurrent requests could both miss and both execute.
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>Returns the cached record for <paramref name="key"/>, or null if no prior successful execution exists.</summary>
    Task<IdempotencyRecord?> GetAsync(string key, CancellationToken ct = default);

    /// <summary>Persists the result of a successful execution under <paramref name="key"/> for <paramref name="expiration"/>.</summary>
    Task SaveAsync(string key, IdempotencyRecord record, TimeSpan expiration, CancellationToken ct = default);
}
