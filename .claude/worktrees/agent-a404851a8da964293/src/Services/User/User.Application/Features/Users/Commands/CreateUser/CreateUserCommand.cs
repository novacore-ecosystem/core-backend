namespace NovaCore.User.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Roles stays a free-form string[] passed straight through to Auth via
/// UserProfileCreatedIntegrationEvent - these are Auth's own Identity role names
/// (AccountRole/Role.Name), not User's local UserRole aggregate, and User service has no
/// visibility into which role names are currently valid. Selecting real Role/Position values
/// (via Auth's own admin-facing Role/Position APIs) is future work - see
/// docs/services/user-service.md.
/// </summary>
public sealed record CreateUserCommand(
    string Email,
    string UserName,
    string PhoneNumber,
    string FirstName,
    string MiddleName,
    string LastName,
    string[] Roles,
    string TempPassword = "") : ICommand<CreateUserResponse>;

public sealed record CreateUserResponse(Guid UserId);
