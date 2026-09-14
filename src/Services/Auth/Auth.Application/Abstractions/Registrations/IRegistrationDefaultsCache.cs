namespace NovaCore.Auth.Application.Abstractions.Registrations;

/// <summary>
/// Read-through cache for RegistrationDefaultsSnapshot, one entry per (Tenant, App) - backs
/// Register's default Role/Permission grant, replacing the former hard-seeded "User" Role
/// dependency.
/// </summary>
/// <remarks>
/// Follows the Effective Authorization / App Collection cache pattern (see
/// docs/reference/caching.md) - an explicit Infrastructure service called directly by
/// RegisterHandler, not a decorator.
/// </remarks>
public interface IRegistrationDefaultsCache
{
    /// <summary>Gets the snapshot for (tenantId, appId), populating the cache on a miss. Never
    /// null - RegistrationDefaultsSnapshot.Empty when nothing is configured.</summary>
    Task<RegistrationDefaultsSnapshot> GetAsync(Guid tenantId, Guid appId, CancellationToken ct = default);

    /// <summary>Removes the cached snapshot - the next GetAsync rebuilds it from the database.</summary>
    Task InvalidateAsync(Guid tenantId, Guid appId, CancellationToken ct = default);

    /// <summary>Eagerly re-reads from persistence and replaces the cached value immediately
    /// (InvalidateAsync + an immediate re-fetch-and-set), for a caller that can't tolerate the
    /// brief window before the next natural read repopulates it (e.g. an admin tool right after
    /// changing defaults).</summary>
    Task RefreshAsync(Guid tenantId, Guid appId, CancellationToken ct = default);
}
