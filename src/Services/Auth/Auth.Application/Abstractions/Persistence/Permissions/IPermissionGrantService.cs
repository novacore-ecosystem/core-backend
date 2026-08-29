using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Permissions;

/// <summary>
/// Generic permission-assignment surface keyed by (ProviderName, ProviderKey) - the centralized
/// replacement for the former Role-only RolePermission mutation on Role.AssignPermission/
/// RemovePermission. Every method validates the requested key against PermissionRegistry.Instance's
/// AllowedProviders for providerName before writing - the server-side security boundary (a client
/// cannot bypass UI/attribute filtering by posting directly).
/// </summary>
public interface IPermissionGrantService
{
    /// <summary>Throws if permissionKey does not exist in the registry, or if providerName is not
    /// one of its AllowedProviders. No-ops if the grant already exists.</summary>
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

    /// <summary>Replaces every grant for one (providerName, providerKey) wholesale - loads the
    /// current grants, resolves the requested keys' PermissionDefinitions, diffs, and applies
    /// grant/revoke internally. A requested key that exists but is not allowed for providerName
    /// throws; a requested key that does not exist at all is silently skipped (matches the prior
    /// Role permission-update endpoint's documented behavior).</summary>
    Task<PermissionGrantReplaceResult> ReplaceForProviderAsync(
        PermissionProviderName providerName,
        string providerKey,
        IReadOnlyCollection<string> permissionKeys,
        Guid tenantId,
        CancellationToken ct = default);
}

public sealed record PermissionGrantReplaceResult(bool HasChanges, IReadOnlySet<string> ResultingKeys);
