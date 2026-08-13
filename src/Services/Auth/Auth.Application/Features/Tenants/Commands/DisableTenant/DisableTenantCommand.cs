namespace NovaCore.Auth.Application.Features.Tenants.Commands.DisableTenant;

public sealed record DisableTenantCommand(Guid Id) : ICommand;
