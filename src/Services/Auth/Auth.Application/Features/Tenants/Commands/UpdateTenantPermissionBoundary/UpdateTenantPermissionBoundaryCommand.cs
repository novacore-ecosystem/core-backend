namespace NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenantPermissionBoundary;

/// <summary>Opts a tenant into (or back out of) permission-boundary enforcement - see
/// TenantMetadata.PermissionBoundaryEnabled and AccountAuthorizationGuard.
/// EnsureWithinTenantBoundary.</summary>
public sealed record UpdateTenantPermissionBoundaryCommand(Guid TenantId, bool Enabled) : ICommand;
