namespace NovaCore.Auth.Application.Features.Auth.Commands.ResetPasswordWithToken;

public record ResetPasswordWithTokenCommand(string Token, string NewPassword) : ICommand;
