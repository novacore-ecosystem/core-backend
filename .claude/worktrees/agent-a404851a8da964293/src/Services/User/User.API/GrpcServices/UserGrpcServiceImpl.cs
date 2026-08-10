using NovaCore.BuildingBlock.Application.Abstractions.Events;
using NovaCore.BuildingBlock.Contract.Protos.User;

using Grpc.Core;

using NovaCore.User.Application.Features.Users.Events.OnUserInitiated;
using NovaCore.User.Application.Features.Users.Queries.GetUserById;

namespace NovaCore.User.API.GrpcServices;

public sealed class UserGrpcServiceImpl(
    IInternalEventDispatcher eventDispatcher,
    ISender sender) : UserGrpcService.UserGrpcServiceBase
{
    public override async Task<CreateUserProfileResponse> CreateUserProfile(
        CreateUserProfileRequest request,
        ServerCallContext context)
    {
        var @event = new OnUserInitiatedEvent(
            Guid.Parse(request.UserId),
            request.Email.Trim(),
            request.UserName.Trim(),
            request.PhoneNumber.Trim(),
            request.FirstName.Trim(),
            request.MiddleName.Trim(),
            request.LastName.Trim(),
            CorrelationId: request.CorrelationId);
        await eventDispatcher.PublishAsync(@event, context.CancellationToken);

        return new CreateUserProfileResponse
        {
            Success = true,
            UserId = @event.AccountId.ToString(),
        };
    }

    public override async Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            return new GetUserResponse { Found = false, UserId = request.UserId };

        var result = await sender.Send(new GetUserByIdQuery(userId), context.CancellationToken);

        return result is null
            ? new GetUserResponse { Found = false, UserId = request.UserId }
            : ToResponse(result);
    }

    public override async Task<GetUsersResponse> GetUsers(GetUsersRequest request, ServerCallContext context)
    {
        var requestedIds = request.UserIds
            .Select(id => Guid.TryParse(id, out var parsed) ? parsed : (Guid?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var results = await sender.Send(new GetUsersByIdsQuery(requestedIds), context.CancellationToken);

        var response = new GetUsersResponse();
        foreach (var idString in request.UserIds)
        {
            // Never omit a requested id, even if it failed to parse or didn't resolve to a real
            // user - found=false with the id echoed back, mirroring inventory.proto's
            // GetProductsStock convention. See docs/tasks/2026-07-28/Task13_grpc-proto-getuser-getusers.md.
            if (Guid.TryParse(idString, out var id) && results.TryGetValue(id, out var result))
                response.Items.Add(ToItem(result));
            else
                response.Items.Add(new UserProfileItem { Found = false, UserId = idString });
        }

        return response;
    }

    private static GetUserResponse ToResponse(UserLookupResult result) => new()
    {
        Found = true,
        UserId = result.UserId.ToString(),
        Email = result.Email,
        UserName = result.UserName,
        FirstName = result.FirstName,
        MiddleName = result.MiddleName,
        LastName = result.LastName,
        DisplayName = result.DisplayName,
        PhoneNumber = result.PhoneNumber,
        Status = result.Status,
        Roles = { result.Roles },
    };

    private static UserProfileItem ToItem(UserLookupResult result) => new()
    {
        Found = true,
        UserId = result.UserId.ToString(),
        Email = result.Email,
        UserName = result.UserName,
        FirstName = result.FirstName,
        MiddleName = result.MiddleName,
        LastName = result.LastName,
        DisplayName = result.DisplayName,
        PhoneNumber = result.PhoneNumber,
        Status = result.Status,
        Roles = { result.Roles },
    };
}
