using NovaCore.Auth.Application.Features.Permissions.Commands.DeletePermission;

using NovaCore.BuildingBlock.Web.Authorization;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.API.Endpoints.PermissionDefinitions;

public sealed class DeletePermissionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/permissions/{id:guid}", async (
            Guid id,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new DeletePermissionCommand(id), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Permissions")
        .RequirePermissions(Permissions.Permission.Manage)
        .WithSummary("Auth_DeletePermission")
        .WithDisplayName("Delete Permission API")
        .Produces<ApiResponse<object>>();
    }
}
