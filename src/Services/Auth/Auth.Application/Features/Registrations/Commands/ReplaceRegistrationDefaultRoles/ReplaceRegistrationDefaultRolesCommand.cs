namespace NovaCore.Auth.Application.Features.Registrations.Commands.ReplaceRegistrationDefaultRoles;

/// <summary>Replaces the caller's Tenant's default Role set for the given App wholesale -
/// RegistrationDefaultsWriteService diffs against the current rows and applies add/remove
/// internally, same replace-by-diff contract as UpdateRolePermissions.</summary>
public sealed record ReplaceRegistrationDefaultRolesCommand(
    Guid AppId, IReadOnlyCollection<Guid> RoleIds) : ICommand;
