using NovaCore.User.Application.Abstractions.Services;

namespace NovaCore.User.Application.Features.Users.Queries.GetUserById;

public sealed class GetUsersByIdsHandler(
    IUserProfileDetailCache userProfileCache,
    IUserDisplayNameFormatter displayNameFormatter) : IQueryHandler<GetUsersByIdsQuery, IReadOnlyDictionary<Guid, UserLookupResult>>
{
    private const string GrpcDisplayLocale = "en";

    public async Task<IReadOnlyDictionary<Guid, UserLookupResult>> Handle(GetUsersByIdsQuery request, CancellationToken ct = default)
    {
        var users = await userProfileCache.GetByIdsAsync(request.UserIds, ct);

        return users.ToDictionary(
            kv => kv.Key,
            kv => new UserLookupResult(
                kv.Value.Id,
                kv.Value.Email,
                kv.Value.UserName,
                kv.Value.PhoneNumber,
                kv.Value.FirstName,
                kv.Value.MiddleName,
                kv.Value.LastName,
                displayNameFormatter.Format(kv.Value.FirstName, kv.Value.MiddleName, kv.Value.LastName, GrpcDisplayLocale),
                kv.Value.Status.ToString(),
                kv.Value.Roles));
    }
}
