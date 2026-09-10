using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Permissions;

/// <summary>
/// Generic permission-assignment surface keyed by (ProviderName, ProviderKey).
/// </summary>
/// <remarks>
/// The centralized replacement for the former Role-only RolePermission mutation on
/// Role.AssignPermission/RemovePermission. Every method validates the requested key against
/// PermissionRegistry.Instance's AllowedProviders for providerName before writing - a client cannot
/// bypass UI/attribute filtering by posting directly.
/// </remarks>
public interface IPermissionGrantService
{
    /// <summary>
    /// Grants a permission, no-op if already granted.
    /// </summary>
    /// <remarks>
    /// Throws if the permission key does not exist in the registry, or if providerName is not one
    /// of its allowed providers.
    /// </remarks>
    Task GrantAsync(
        string permissionKey,
        PermissionProviderName providerName,
        string providerKey,
        Guid tenantId,
        CancellationToken ct = default);

    Task RevokeAsync(
        string permissionKey,
        PermissionProviderName providerName,
        string providerKey,
        Guid tenantId,
        CancellationToken ct = default);

    /// <summary>
    /// Replaces every grant for one (providerName, providerKey) with the given set.
    /// </summary>
    /// <remarks>
    /// A requested key that exists but is not allowed for providerName throws; a requested key
    /// that does not exist at all is silently skipped.
    /// </remarks>
    /// <param name="permissionKeys">The desired end-state permission set.</param>
    Task<PermissionGrantReplaceResult> ReplaceForProviderAsync(
        PermissionProviderName providerName,
        string providerKey,
        IReadOnlyCollection<string> permissionKeys,
        Guid tenantId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the permission keys currently granted to one (providerName, providerKey).
    /// </summary>
    Task<IReadOnlySet<string>> GetGrantedKeysAsync(
        PermissionProviderName providerName,
        string providerKey,
        Guid tenantId,
        CancellationToken ct = default);
}

public sealed record PermissionGrantReplaceResult(bool HasChanges, IReadOnlySet<string> ResultingKeys);
