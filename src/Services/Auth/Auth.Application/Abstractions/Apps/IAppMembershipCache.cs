namespace NovaCore.Auth.Application.Abstractions.Apps;

/// <summary>
/// Read-through cache of each App's AccountID membership set, for O(1) "is this account assigned
/// to this App" checks without a database round trip on every login/refresh.
/// </summary>
/// <remarks>
/// Follows the Effective Authorization / User Detail cache pattern (see docs/reference/caching.md)
/// - an explicit Infrastructure service called directly by callers, not a decorator.
/// </remarks>
public interface IAppMembershipCache
{
    /// <summary>Whether the account is a member of the App, populating that App's membership
    /// cache on a miss.</summary>
    /// <param name="appId">The App to check membership against.</param>
    /// <param name="accountId">The account to check.</param>
    Task<bool> IsAssignedAsync(Guid appId, Guid accountId, CancellationToken ct = default);

    /// <summary>Removes the App's cached membership set, e.g. after an account is registered
    /// into, assigned to, or removed from it - the next lookup rebuilds it from the database.</summary>
    /// <param name="appId">The App whose membership set is now stale.</param>
    Task InvalidateAsync(Guid appId, CancellationToken ct = default);
}
