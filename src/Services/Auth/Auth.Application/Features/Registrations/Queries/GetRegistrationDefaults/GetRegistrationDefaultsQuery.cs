namespace NovaCore.Auth.Application.Features.Registrations.Queries.GetRegistrationDefaults;

public sealed record GetRegistrationDefaultsQuery(Guid AppId) : IQuery<RegistrationDefaultsResponse>;

/// <summary>
/// The default RoleIds/PermissionKeys configured for the caller's Tenant and the given App
/// the same lightweight shape RegisterHandler consumes, not a duplicated read model.
/// </summary>
public sealed record RegistrationDefaultsResponse(
    IReadOnlyCollection<Guid> RoleIds,
    IReadOnlyCollection<string> PermissionKeys);
