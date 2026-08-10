using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using NovaCore.Notification.Application.Features.NotificationChannels.Commands.EnableNotificationChannel;

namespace NovaCore.Notification.API.Endpoints.NotificationChannel;

public sealed class EnableChannel : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/notification-channels/{channelId}/enable", EnableAsync)
            .WithTags("NotificationChannel")
            .RequirePermissions(Permissions.Notification.ChannelToggle)
            .WithName("EnableNotificationChannel")
            .WithDisplayName("Enable Notification Channel API")
            .WithDescription("Enables a channel. Requires the configuration to already be Valid (see RecordValidationResult).")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> EnableAsync(
        [FromRoute] Guid channelId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        await sender.Send(new EnableNotificationChannelCommand(channelId), ct);
        return Results.Ok(ApiResponse<object>.Ok());
    }
}
