using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using NovaCore.Notification.Application.Features.NotificationGroups.Queries.GetNotificationGroup;

namespace NovaCore.Notification.API.Endpoints.NotificationGroup;

public sealed class GetGroup : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/notification-groups/{groupId}", GetAsync)
            .WithTags("NotificationGroup")
            .RequirePermissions(Permissions.Notification.View)
            .WithName("GetNotificationGroup")
            .WithDisplayName("Get Notification Group API")
            .Produces<ApiResponse<GetNotificationGroupResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid groupId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new GetNotificationGroupQuery(groupId), ct);
        return Results.Ok(ApiResponse<GetNotificationGroupResponse>.Ok(response));
    }
}
