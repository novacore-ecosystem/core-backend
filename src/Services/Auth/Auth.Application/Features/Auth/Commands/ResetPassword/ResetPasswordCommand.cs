namespace NovaCore.Auth.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(string CurrentPassword, string NewPassword) : ICommand;
