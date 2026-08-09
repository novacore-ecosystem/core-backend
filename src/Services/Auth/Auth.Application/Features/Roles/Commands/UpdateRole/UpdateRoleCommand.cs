namespace NovaCore.Auth.Application.Features.Roles.Commands.UpdateRole;

public sealed record UpdateRoleCommand(Guid Id, string Name, string? Description) : ICommand;
