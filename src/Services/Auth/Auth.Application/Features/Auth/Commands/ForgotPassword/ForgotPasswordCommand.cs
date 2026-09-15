namespace NovaCore.Auth.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : ICommand;
