namespace NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenant;

public sealed record UpdateTenantCommand(
    Guid Id,
    string Name,
    string? LogoUrl,
    string? FaviconUrl) : ICommand;
