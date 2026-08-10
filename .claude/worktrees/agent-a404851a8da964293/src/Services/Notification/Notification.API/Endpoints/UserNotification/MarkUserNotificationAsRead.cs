using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using NovaCore.Notification.Application.Features.UserNotifications.Commands.MarkUserNotificationAsRead;

namespace NovaCore.Notification.API.Endpoints.UserNotification;

/// <summary>Scoped to the calling user - enforced in the handler, callers may only mark their own as read.</summary>
public sealed class MarkUserNotificationAsRead : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/user-notifications/{notificationId}/read", HandleAsync)
            .WithTags("UserNotification")
            .RequireAuthorization()
            .WithName("MarkUserNotificationAsRead")
            .WithDisplayName("Mark User Notification As Read API")
            .WithDescription("Marks one of the caller's own Notification Center entries as read.")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid notificationId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        await sender.Send(new MarkUserNotificationAsReadCommand(notificationId), ct);
        return Results.Ok(ApiResponse<object>.Ok());
    }
}
