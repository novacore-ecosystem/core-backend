namespace NovaCore.Auth.Application.Abstractions.Registrations;

/// <summary>What RegisterHandler needs to grant a newly self-registered Account, for one
/// (Tenant, App) pair - RoleIds go straight into IAccountRoleAssignmentService.ReplaceRolesAsync,
/// PermissionKeys go straight into IPermissionGrantService.ReplaceForProviderAsync. Both
/// collections are empty (not null) when nothing is configured - callers never branch on
/// "missing config", they just pass the (possibly empty) collections through.</summary>
public sealed record RegistrationDefaultsSnapshot(
    IReadOnlyCollection<Guid> RoleIds,
    IReadOnlyCollection<string> PermissionKeys)
{
    public static readonly RegistrationDefaultsSnapshot Empty = new([], []);
}
