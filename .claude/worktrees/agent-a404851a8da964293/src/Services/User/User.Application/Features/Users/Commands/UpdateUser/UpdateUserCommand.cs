namespace NovaCore.User.Application.Features.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string FirstName,
    string MiddleName,
    string LastName,
    string PhoneNumber) : ICommand<UpdateUserResponse>;

public sealed record UpdateUserResponse;
