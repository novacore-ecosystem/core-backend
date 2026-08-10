namespace NovaCore.User.Application.Features.Users.Queries.GetUserById;

/// <summary>Shared response shape for the gRPC GetUser/GetUsers RPCs - see docs/reference/grpc.md.</summary>
public sealed record UserLookupResult(
    Guid UserId,
    string Email,
    string UserName,
    string PhoneNumber,
    string FirstName,
    string MiddleName,
    string LastName,
    string DisplayName,
    string Status,
    IReadOnlyList<string> Roles);
