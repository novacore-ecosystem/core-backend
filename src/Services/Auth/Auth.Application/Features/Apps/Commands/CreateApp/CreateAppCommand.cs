namespace NovaCore.Auth.Application.Features.Apps.Commands.CreateApp;

public sealed record CreateAppCommand(string Code, string Name) : ICommand<Guid>;
