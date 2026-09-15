namespace NovaCore.Auth.Application.Features.Auth.Commands.ConfirmEmail;

public record ConfirmEmailCommand(Guid AccountId, string Token) : ICommand;
