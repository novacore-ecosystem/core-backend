using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using NovaCore.Notification.Application.Features.NotificationDispatches.Queries.ListNotificationDispatches;

namespace NovaCore.Notification.API.Endpoints.NotificationDispatch;

public sealed class ListDispatches : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/notification-dispatches", ListAsync)
            .WithTags("NotificationDispatch")
            .RequirePermissions(Permissions.Notification.View)
            .WithName("ListNotificationDispatches")
            .WithDisplayName("List Notification Dispatches API")
            .Produces<ApiResponse<PaginatedResult<NotificationDispatchSummaryResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] DispatchStatus? status,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new ListNotificationDispatchesQuery(
            status,
            page is null or <= 0 ? 1 : page.Value,
            pageSize is null or <= 0 ? 20 : pageSize.Value);

        var response = await sender.Send(query, ct);
        return Results.Ok(ApiResponse<PaginatedResult<NotificationDispatchSummaryResponse>>.Ok(response));
    }
}
