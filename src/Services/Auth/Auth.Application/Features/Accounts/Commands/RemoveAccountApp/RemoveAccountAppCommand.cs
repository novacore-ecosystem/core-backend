namespace NovaCore.Auth.Application.Features.Accounts.Commands.RemoveAccountApp;

public sealed record RemoveAccountAppCommand(Guid AccountId, Guid AppId) : ICommand;
