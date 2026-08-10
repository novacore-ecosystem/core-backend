using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using NovaCore.Notification.Application.Features.NotificationChannels.Queries.GetNotificationChannel;

namespace NovaCore.Notification.API.Endpoints.NotificationChannel;

public sealed class GetChannel : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/notification-channels/{channelId}", GetAsync)
            .WithTags("NotificationChannel")
            .RequirePermissions(Permissions.Notification.View)
            .WithName("GetNotificationChannel")
            .WithDisplayName("Get Notification Channel API")
            .Produces<ApiResponse<GetNotificationChannelResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid channelId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new GetNotificationChannelQuery(channelId), ct);
        return Results.Ok(ApiResponse<GetNotificationChannelResponse>.Ok(response));
    }
}
