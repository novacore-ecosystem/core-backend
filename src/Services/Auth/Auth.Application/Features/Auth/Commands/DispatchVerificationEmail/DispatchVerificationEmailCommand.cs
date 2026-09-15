namespace NovaCore.Auth.Application.Features.Auth.Commands.DispatchVerificationEmail;

public record DispatchVerificationEmailCommand(string Email) : ICommand;
