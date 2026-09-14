namespace NovaCore.Auth.Application.Features.Apps.Commands.UpdateApp;

public sealed record UpdateAppCommand(Guid Id, string Name, bool IsActive) : ICommand;
