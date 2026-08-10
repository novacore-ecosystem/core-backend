namespace NovaCore.User.Application.Features.Users.Commands.DeleteUser;

public sealed record DeleteUserCommand(
    Guid UserId,
    string Reason,
    string CorrelationId) : ICommand<DeleteUserProfileResponse>;

public sealed record DeleteUserProfileResponse(bool Deleted);
