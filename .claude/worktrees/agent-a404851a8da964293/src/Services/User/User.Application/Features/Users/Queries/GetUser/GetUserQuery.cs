namespace NovaCore.User.Application.Features.Users.Queries.GetUser;

public sealed record GetUserQuery(Guid UserId) : IQuery<GetUserResponse>;

public sealed record GetUserResponse(
    Guid Id,
    string Email,
    string UserName,
    string PhoneNumber,
    string FirstName,
    string MiddleName,
    string LastName,
    string DisplayName,
    UserStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
