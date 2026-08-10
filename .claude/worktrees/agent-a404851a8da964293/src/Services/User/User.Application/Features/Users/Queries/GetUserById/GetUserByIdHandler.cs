using NovaCore.User.Application.Abstractions.Services;

namespace NovaCore.User.Application.Features.Users.Queries.GetUserById;

public sealed class GetUserByIdHandler(
    IUserProfileDetailCache userProfileCache,
    IUserDisplayNameFormatter displayNameFormatter) : IQueryHandler<GetUserByIdQuery, UserLookupResult?>
{
    // gRPC callers are services, not an authenticated HTTP request with its own Accept-Language -
    // fixed default locale, matching the search index's same simplification. See
    // docs/tasks/2026-07-28/Task13_grpc-proto-getuser-getusers.md.
    private const string GrpcDisplayLocale = "en";

    public async Task<UserLookupResult?> Handle(GetUserByIdQuery request, CancellationToken ct = default)
    {
        var user = await userProfileCache.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return null;

        return new UserLookupResult(
            user.Id,
            user.Email,
            user.UserName,
            user.PhoneNumber,
            user.FirstName,
            user.MiddleName,
            user.LastName,
            displayNameFormatter.Format(user.FirstName, user.MiddleName, user.LastName, GrpcDisplayLocale),
            user.Status.ToString(),
            user.Roles);
    }
}
