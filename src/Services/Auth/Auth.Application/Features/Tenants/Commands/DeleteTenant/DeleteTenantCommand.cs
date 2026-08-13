namespace NovaCore.Auth.Application.Features.Tenants.Commands.DeleteTenant;

public sealed record DeleteTenantCommand(Guid Id) : ICommand;
