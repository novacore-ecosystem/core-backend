namespace NovaCore.Auth.Application.Features.Roles.Commands.CreateRole;

public sealed record CreateRoleCommand(string Name, string Code, string? Description) : ICommand<Guid>;
