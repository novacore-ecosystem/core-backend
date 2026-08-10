namespace NovaCore.BuildingBlock.Application.Abstractions.Idempotency;

/// <summary>
/// A held distributed lock. Disposing (or calling <see cref="ReleaseAsync"/>) releases it -
/// callers should acquire this via <c>await using</c> so release always runs, even on exception.
/// </summary>
public interface IDistributedLock : IAsyncDisposable
{
    /// <summary>The resource key this lock was acquired for.</summary>
    string Resource { get; }

    /// <summary>Releases the lock. Safe to call more than once; a second call is a no-op.</summary>
    Task ReleaseAsync(CancellationToken ct = default);
}
