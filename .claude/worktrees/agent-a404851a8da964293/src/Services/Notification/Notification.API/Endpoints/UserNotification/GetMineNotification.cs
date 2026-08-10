using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using NovaCore.Notification.Application.Features.UserNotifications.Queries.ListMyUserNotifications;

namespace NovaCore.Notification.API.Endpoints.UserNotification;

/// <summary>Cursor/lazy-load paginated, scoped to the calling user - enforced in the handler.</summary>
public sealed class GetMineNotification : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/user-notifications/me", HandleAsync)
            .WithTags("UserNotification")
            .RequireAuthorization()
            .WithName("ListMyUserNotifications")
            .WithDisplayName("List My User Notifications API")
            .WithDescription("Cursor-paginated, filterable (status) list of the caller's own Notification Center entries, newest first. Pass the previous response's nextCursor to fetch the next page; omit for the first page.")
            .Produces<ApiResponse<CursorPaginatedResult<UserNotificationSummaryResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        [FromQuery] NotificationStatus? status,
        [FromQuery] string? cursor,
        [FromQuery] int? limit,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new ListMyUserNotificationsQuery(
            status,
            cursor,
            limit is null or <= 0 ? 20 : limit.Value);

        var response = await sender.Send(query, ct);
        return Results.Ok(ApiResponse<CursorPaginatedResult<UserNotificationSummaryResponse>>.Ok(response));
    }
}
