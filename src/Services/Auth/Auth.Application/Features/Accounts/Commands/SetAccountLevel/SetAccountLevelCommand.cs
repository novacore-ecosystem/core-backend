namespace NovaCore.Auth.Application.Features.Accounts.Commands.SetAccountLevel;

public sealed record SetAccountLevelCommand(Guid AccountId, int Level) : ICommand;
