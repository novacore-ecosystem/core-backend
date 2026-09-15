namespace NovaCore.Auth.Application.Features.Auth.Commands.ResendEmail;

public record ResendEmailCommand(string Email, AuthMailPurpose Purpose) : ICommand;
