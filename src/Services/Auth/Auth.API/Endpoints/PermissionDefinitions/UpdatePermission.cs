using NovaCore.Auth.Application.Features.Permissions.Commands.UpdatePermission;

using NovaCore.BuildingBlock.Web.Authorization;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.API.Endpoints.PermissionDefinitions;

public record UpdatePermissionRequest(Guid PermissionGroupId);

public sealed class UpdatePermissionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/permissions/{id:guid}", async (
            Guid id,
            [FromBody] UpdatePermissionRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new UpdatePermissionCommand(id, request.PermissionGroupId), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Permissions")
        .RequirePermissions(Permissions.Permission.Manage)
        .WithSummary("Auth_UpdatePermission")
        .WithDisplayName("Update Permission API")
        .Produces<ApiResponse<object>>();
    }
}
