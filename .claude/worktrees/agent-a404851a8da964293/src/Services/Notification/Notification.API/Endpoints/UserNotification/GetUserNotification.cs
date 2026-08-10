using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using NovaCore.Notification.Application.Features.UserNotifications.Queries.GetUserNotification;

namespace NovaCore.Notification.API.Endpoints.UserNotification;

/// <summary>Scoped to the calling user - enforced in the handler, callers may only fetch their own.</summary>
public sealed class GetUserNotification : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/user-notifications/{notificationId}", HandleAsync)
            .WithTags("UserNotification")
            .RequireAuthorization()
            .WithName("GetUserNotification")
            .WithDisplayName("Get User Notification API")
            .WithDescription("Fetches one Notification Center entry. Callers may only fetch their own.")
            .Produces<ApiResponse<GetUserNotificationResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid notificationId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new GetUserNotificationQuery(notificationId), ct);
        return Results.Ok(ApiResponse<GetUserNotificationResponse>.Ok(response));
    }
}
