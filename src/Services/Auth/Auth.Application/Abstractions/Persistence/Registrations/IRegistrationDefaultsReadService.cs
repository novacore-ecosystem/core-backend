namespace NovaCore.Auth.Application.Abstractions.Persistence.Registrations;

/// <summary>
/// Read-only access to the default Roles/Permissions configured for one (Tenant, App) pair -
/// what a newly self-registered account should receive. Backs
/// <see cref="Abstractions.Registrations.IRegistrationDefaultsCache"/> on a cache miss.
/// </summary>
public interface IRegistrationDefaultsReadService
{
    /// <summary>Every default RoleId configured for this (tenant, app) pair. Empty, never null,
    /// when nothing is configured.</summary>
    Task<IReadOnlyCollection<Guid>> GetDefaultRoleIdsAsync(Guid tenantId, Guid appId, CancellationToken ct = default);

    /// <summary>Every default permission KEY (not id - resolved here via the PermissionDefinition
    /// join, one round trip) configured for this (tenant, app) pair - the exact shape
    /// IPermissionGrantService.ReplaceForProviderAsync needs, no second resolution step required.
    /// Empty, never null, when nothing is configured.</summary>
    Task<IReadOnlyCollection<string>> GetDefaultPermissionKeysAsync(Guid tenantId, Guid appId, CancellationToken ct = default);
}
