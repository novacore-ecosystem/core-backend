namespace NovaCore.Auth.Application.Abstractions.Persistence.Accounts;

/// <summary>
/// Direct App membership assignment on an Account - the relationship Login/Refresh validate
/// against.
/// </summary>
public interface IAccountAppAssignmentService
{
    /// <summary>
    /// Assigns the account to the App. Idempotent - already-assigned is a no-op, not an error,
    /// since registration and an explicit Root assignment can both legitimately race for the
    /// same pair.
    /// </summary>
    /// <param name="accountId">The account being assigned.</param>
    /// <param name="appId">The App to assign it to.</param>
    Task AssignAsync(Guid accountId, Guid appId, CancellationToken ct = default);

    /// <summary>
    /// Removes the account's membership in the App. Idempotent - a missing assignment is a
    /// no-op, not an error.
    /// </summary>
    /// <param name="accountId">The account being managed.</param>
    /// <param name="appId">The App to remove membership from.</param>
    Task RemoveAsync(Guid accountId, Guid appId, CancellationToken ct = default);

    /// <summary>
    /// Whether the account currently holds membership in the App.
    /// </summary>
    /// <param name="accountId">The account to check.</param>
    /// <param name="appId">The App to check membership against.</param>
    Task<bool> IsAssignedAsync(Guid accountId, Guid appId, CancellationToken ct = default);

    /// <summary>
    /// Every AccountID currently assigned to the App, in one batch query - backs
    /// IAppMembershipCache's per-App membership set on a cache miss.
    /// </summary>
    /// <param name="appId">The App to look up.</param>
    Task<IReadOnlyCollection<Guid>> GetAccountIdsByAppAsync(Guid appId, CancellationToken ct = default);
}
