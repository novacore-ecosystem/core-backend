using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using NovaCore.Notification.Application.Features.NotificationGroups.Commands.CreateNotificationGroup;

namespace NovaCore.Notification.API.Endpoints.NotificationGroup;

public sealed class CreateGroup : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/notification-groups", CreateAsync)
            .WithTags("NotificationGroup")
            .RequirePermissions(Permissions.Notification.Manage)
            .WithName("CreateNotificationGroup")
            .WithDisplayName("Create Notification Group API")
            .WithDescription("Creates a target audience (role, specific users, segment, ...) that campaigns broadcast to.")
            .Produces<ApiResponse<CreateNotificationGroupResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateNotificationGroupCommand command,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(command, ct);
        return Results.Ok(ApiResponse<CreateNotificationGroupResponse>.Ok(response));
    }
}
