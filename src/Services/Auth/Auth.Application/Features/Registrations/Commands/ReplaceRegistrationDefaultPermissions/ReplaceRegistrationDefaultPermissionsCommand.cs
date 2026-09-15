namespace NovaCore.Auth.Application.Features.Registrations.Commands.ReplaceRegistrationDefaultPermissions;

/// <summary>
/// Replaces the caller's Tenant's default permission set for the given App wholesale -
/// RegistrationDefaultsWriteService diffs against the current rows and applies add/remove
/// internally, same replace-by-diff contract as UpdateRolePermissions.
/// </summary>
public sealed record ReplaceRegistrationDefaultPermissionsCommand(
    Guid AppId,
    IReadOnlyCollection<string> PermissionKeys) : ICommand;
