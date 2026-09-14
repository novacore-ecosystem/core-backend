namespace NovaCore.Auth.Application.Features.Accounts.Commands.AssignAccountApp;

public sealed record AssignAccountAppCommand(Guid AccountId, Guid AppId) : ICommand;
