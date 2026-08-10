namespace NovaCore.Auth.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : ICommand<LoginResult>;

public record LoginResult(string AccessToken, string RefreshToken);
