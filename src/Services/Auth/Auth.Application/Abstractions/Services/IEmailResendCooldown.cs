namespace NovaCore.Auth.Application.Abstractions.Services;

/// <summary>Outcome of <see cref="IEmailResendCooldown.TryClaimAsync"/>.</summary>
/// <param name="Claimed">Whether the caller now owns the cooldown window and may proceed.</param>
/// <param name="RemainingSeconds">Seconds until the window clears, when not claimed.</param>
public readonly record struct CooldownClaim(bool Claimed, int RemainingSeconds);

/// <summary>
/// Per-key anti-spam window, atomic across concurrent requests and Auth Service instances.
/// Backed by Redis SET NX so a claim and a check-and-reject can never both succeed for the
/// same key within the same window.
/// </summary>
public interface IEmailResendCooldown
{
    /// <summary>
    /// Atomically claims <paramref name="key"/> for <paramref name="window"/> if it is free.
    /// </summary>
    /// <returns>A claimed result the caller may act on, or a rejected one carrying the remaining seconds.</returns>
    Task<CooldownClaim> TryClaimAsync(string key, TimeSpan window, CancellationToken ct = default);

    /// <summary>Releases a claim early - used when the work behind it failed, so a genuine retry isn't penalized for an attempt that never actually sent anything.</summary>
    Task ReleaseAsync(string key, CancellationToken ct = default);
}
