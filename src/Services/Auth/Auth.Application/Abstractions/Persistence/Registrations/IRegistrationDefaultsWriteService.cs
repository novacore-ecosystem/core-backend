namespace NovaCore.Auth.Application.Abstractions.Persistence.Registrations;

/// <summary>
/// Minimal write surface for configuring one (Tenant, App) pair's registration defaults -
/// no HTTP endpoint exists yet, this exists for tests and future admin tooling.
/// </summary>
public interface IRegistrationDefaultsWriteService
{
    /// <summary>Replaces every default Role for (tenantId, appId) with the given set - same
    /// replace-by-diff contract as IAccountRoleAssignmentService.ReplaceRolesAsync, applied at
    /// the (Tenant, App) grain instead of the Account grain. An unknown RoleId is silently
    /// skipped.</summary>
    Task ReplaceDefaultRolesAsync(
        Guid tenantId, Guid appId, IReadOnlyCollection<Guid> roleIds, CancellationToken ct = default);

    /// <summary>Replaces every default permission for (tenantId, appId) with the given set -
    /// same replace-by-diff contract as IPermissionGrantService.ReplaceForProviderAsync. A key
    /// that exists but isn't allowed for PermissionProviderName.User throws (validated up front,
    /// so a bad default is rejected here, not discovered the first time a user registers); an
    /// unknown key is silently skipped.</summary>
    Task ReplaceDefaultPermissionsAsync(
        Guid tenantId, Guid appId, IReadOnlyCollection<string> permissionKeys, CancellationToken ct = default);
}
