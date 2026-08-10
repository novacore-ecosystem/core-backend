namespace NovaCore.Auth.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string MiddleName = "") : ICommand<RegisterResult>;

public record RegisterResult;
