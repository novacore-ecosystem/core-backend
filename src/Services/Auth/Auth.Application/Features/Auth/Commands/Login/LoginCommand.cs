namespace NovaCore.Auth.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password,
    string ClientPublicKey,
    string AppCode) : ICommand<LoginResult>;

public record LoginResult(string AccessToken, string RefreshToken);
