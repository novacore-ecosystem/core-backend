using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

using Mapster;

using NovaCore.User.Application.Abstractions.Services;

namespace NovaCore.User.Application.Features.Users.Queries.GetUser;

public sealed class GetUserHandler(
    IUserReadService userReadService,
    IUserDisplayNameFormatter displayNameFormatter,
    ICurrentLocaleService currentLocale) : IQueryHandler<GetUserQuery, GetUserResponse>
{
    public async Task<GetUserResponse> Handle(GetUserQuery request, CancellationToken ct = default)
    {
        var user = await userReadService.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        var displayName = displayNameFormatter.Format(user.FirstName, user.MiddleName, user.LastName, currentLocale.GetLocale());

        return user.Adapt<GetUserResponse>() with { DisplayName = displayName };
    }
}
