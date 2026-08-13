namespace NovaCore.Auth.Application.Features.Tenants.Commands.CreateTenant;

public sealed record CreateTenantCommand(
    string Code,
    string Name,
    string? LogoUrl,
    string? FaviconUrl) : ICommand<Guid>;
