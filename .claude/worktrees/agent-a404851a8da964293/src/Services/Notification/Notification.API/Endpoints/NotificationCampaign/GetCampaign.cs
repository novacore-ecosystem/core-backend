using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using NovaCore.Notification.Application.Features.NotificationCampaigns.Queries.GetNotificationCampaign;

namespace NovaCore.Notification.API.Endpoints.NotificationCampaign;

public sealed class GetCampaign : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/notification-campaigns/{campaignId}", HandleAsync)
            .WithTags("NotificationCampaign")
            .RequirePermissions(Permissions.Notification.View)
            .WithName("GetNotificationCampaign")
            .WithDisplayName("Get Notification Campaign API")
            .Produces<ApiResponse<GetNotificationCampaignResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid campaignId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new GetNotificationCampaignQuery(campaignId), ct);
        return Results.Ok(ApiResponse<GetNotificationCampaignResponse>.Ok(response));
    }
}
