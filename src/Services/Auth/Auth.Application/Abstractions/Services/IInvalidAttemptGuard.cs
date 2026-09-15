namespace NovaCore.Auth.Application.Abstractions.Services;

/// <summary>Outcome of a lock check/failure record against <see cref="IInvalidAttemptGuard"/>.</summary>
/// <param name="Locked">Whether the key is currently locked out.</param>
/// <param name="RemainingSeconds">Seconds until the lock clears, when locked.</param>
public readonly record struct AttemptLockStatus(bool Locked, int RemainingSeconds);

/// <summary>
/// Per-key invalid-attempt counter with a lockout, atomic across concurrent requests and Auth
/// Service instances. Used by code/token validation flows (email confirmation, password reset)
/// to stop repeated wrong guesses against the same key - independent of
/// <see cref="IEmailResendCooldown"/>, which only governs how often a fresh code can be sent.
/// </summary>
public interface IInvalidAttemptGuard
{
    /// <summary>Checks whether <paramref name="key"/> is currently locked, without recording an attempt.</summary>
    Task<AttemptLockStatus> CheckLockAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Atomically records one invalid attempt for <paramref name="key"/>. Once the running count
    /// reaches <paramref name="maxAttempts"/>, the key is locked for <paramref name="lockDuration"/>
    /// and the counter is cleared - the triggering attempt itself is the one that locks the key,
    /// it does not get a free extra try.
    /// </summary>
    Task<AttemptLockStatus> RecordFailureAsync(string key, int maxAttempts, TimeSpan lockDuration, CancellationToken ct = default);

    /// <summary>Clears both the attempt counter and any active lock for <paramref name="key"/> - called after a successful validation.</summary>
    Task ResetAsync(string key, CancellationToken ct = default);
}
