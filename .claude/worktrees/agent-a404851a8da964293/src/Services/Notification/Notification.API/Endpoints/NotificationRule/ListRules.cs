using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using NovaCore.Notification.Application.Features.NotificationRules.Queries.ListNotificationRules;

namespace NovaCore.Notification.API.Endpoints.NotificationRule;

public sealed class ListRules : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/notification-rules", ListAsync)
            .WithTags("NotificationRule")
            .RequirePermissions(Permissions.Notification.View)
            .WithName("ListNotificationRules")
            .WithDisplayName("List Notification Rules API")
            .Produces<ApiResponse<PaginatedResult<NotificationRuleSummaryResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] string? eventType,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new ListNotificationRulesQuery(
            eventType,
            page is null or <= 0 ? 1 : page.Value,
            pageSize is null or <= 0 ? 20 : pageSize.Value);

        var response = await sender.Send(query, ct);
        return Results.Ok(ApiResponse<PaginatedResult<NotificationRuleSummaryResponse>>.Ok(response));
    }
}
