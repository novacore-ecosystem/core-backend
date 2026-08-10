using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using NovaCore.Notification.Application.Features.UserNotifications.Commands.CreateUserNotification;

namespace NovaCore.Notification.API.Endpoints.UserNotification;

/// <summary>Admin-only - no automatic rule/campaign trigger is wired up yet to generate these.</summary>
public sealed class CreateUserNotification : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/user-notifications", HandleAsync)
            .WithTags("UserNotification")
            .RequirePermissions(Permissions.Notification.Send)
            .WithName("CreateUserNotification")
            .WithDisplayName("Create User Notification API")
            .WithDescription("Creates a Notification Center entry for a user. Admin only - no automatic rule/campaign trigger is wired up yet.")
            .Produces<ApiResponse<CreateUserNotificationResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        [FromBody] CreateUserNotificationCommand command,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(command, ct);
        return Results.Ok(ApiResponse<CreateUserNotificationResponse>.Ok(response));
    }
}
