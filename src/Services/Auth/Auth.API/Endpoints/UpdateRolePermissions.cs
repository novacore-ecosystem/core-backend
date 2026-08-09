using NovaCore.Auth.Application.Features.Roles.Commands.UpdateRolePermissions;

using NovaCore.BuildingBlock.Web.Authorization;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.API.Endpoints;

public record UpdateRolePermissionsRequest(IReadOnlyCollection<string> PermissionKeys);

public sealed class UpdateRolePermissions : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/roles/{id:guid}/permissions", async (
            Guid id,
            [FromBody] UpdateRolePermissionsRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new UpdateRolePermissionsCommand(id, request.PermissionKeys), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Roles")
        .RequirePermissions(Permissions.Role.Manage)
        .WithSummary("Auth_UpdateRolePermissions")
        .WithDisplayName("Update Role Permissions API")
        .Produces<ApiResponse<object>>();
    }
}
