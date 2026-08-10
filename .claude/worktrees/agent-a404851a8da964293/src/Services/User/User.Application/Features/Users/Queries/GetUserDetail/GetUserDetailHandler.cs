using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.User.Application.Abstractions.Services;

namespace NovaCore.User.Application.Features.Users.Queries.GetUserDetail;

public sealed class GetUserDetailHandler(
    ICurrentUserService currentUser,
    ICurrentLocaleService currentLocale,
    IUserProfileDetailCache userProfileCache,
    IRoleCacheReader roleCacheReader,
    IUserDisplayNameFormatter displayNameFormatter) : IQueryHandler<GetUserDetailQuery, GetUserDetailResponse>
{
    public async Task<GetUserDetailResponse> Handle(GetUserDetailQuery request, CancellationToken ct = default)
    {
        var userId = currentUser.GetUserId()
            ?? throw new UnauthorizedException();

        var user = await userProfileCache.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("UserProfile", userId);

        // Roles are read from Auth's own role cache (IRoleCacheReader), not the User Detail
        // cache below - kept unchanged from before this cache existed, since they're two
        // different caches with two different owners (see docs/reference/caching.md).
        var roles = await roleCacheReader.GetUserRolesAsync(userId, ct);
        var displayName = displayNameFormatter.Format(user.FirstName, user.MiddleName, user.LastName, currentLocale.GetLocale());

        return new GetUserDetailResponse(
            user.Id,
            user.Email,
            user.UserName,
            user.PhoneNumber,
            user.FirstName,
            user.MiddleName,
            user.LastName,
            displayName,
            user.Status,
            roles,
            user.CreatedAt,
            user.UpdatedAt);
    }
}
