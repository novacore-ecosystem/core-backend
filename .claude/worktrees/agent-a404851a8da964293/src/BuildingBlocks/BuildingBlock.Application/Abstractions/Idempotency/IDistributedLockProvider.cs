namespace NovaCore.BuildingBlock.Application.Abstractions.Idempotency;

/// <summary>
/// Provides mutual exclusion across service instances. Purely about "only one caller holds this
/// resource at a time" - it has no notion of whether the guarded operation has already run before;
/// see <see cref="IIdempotencyStore"/> for that half of idempotency.
/// </summary>
public interface IDistributedLockProvider
{
    /// <summary>
    /// Attempts to acquire a lock on <paramref name="resource"/>, waiting up to <paramref name="timeout"/>.
    /// Returns null if the lock could not be acquired within that window (caller should treat this as
    /// "another request for the same operation is already in flight", not an error to retry blindly).
    /// </summary>
    /// <param name="resource">Unique key identifying the operation being guarded.</param>
    /// <param name="expiration">Safety-net TTL applied to the lock itself, in case the holder crashes without releasing.</param>
    /// <param name="timeout">Maximum time to wait to acquire the lock before giving up.</param>
    Task<IDistributedLock?> AcquireAsync(
        string resource,
        TimeSpan expiration,
        TimeSpan timeout,
        CancellationToken ct = default);
}
