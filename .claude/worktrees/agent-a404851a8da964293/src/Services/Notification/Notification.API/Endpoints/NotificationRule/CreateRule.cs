using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using NovaCore.Notification.Application.Features.NotificationRules.Commands.CreateNotificationRule;

namespace NovaCore.Notification.API.Endpoints.NotificationRule;

public sealed class CreateRule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/notification-rules", CreateAsync)
            .WithTags("NotificationRule")
            .RequirePermissions(Permissions.Notification.Manage)
            .WithName("CreateNotificationRule")
            .WithDisplayName("Create Notification Rule API")
            .WithDescription("Defines what notification actions to create when a given business event occurs (e.g. OrderCreated -> User Notification + Email + Telegram).")
            .Produces<ApiResponse<CreateNotificationRuleResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateNotificationRuleCommand command,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(command, ct);
        return Results.Ok(ApiResponse<CreateNotificationRuleResponse>.Ok(response));
    }
}
