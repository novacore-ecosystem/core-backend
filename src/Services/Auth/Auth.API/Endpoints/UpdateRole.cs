using NovaCore.Auth.Application.Features.Roles.Commands.UpdateRole;

using NovaCore.BuildingBlock.Web.Authorization;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.API.Endpoints;

public record UpdateRoleRequest(string Name, string? Description);

public sealed class UpdateRole : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/roles/{id:guid}", async (
            Guid id,
            [FromBody] UpdateRoleRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new UpdateRoleCommand(id, request.Name, request.Description), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Roles")
        .RequirePermissions(Permissions.Role.Manage)
        .WithSummary("Auth_UpdateRole")
        .WithDisplayName("Update Role API")
        .Produces<ApiResponse<object>>();
    }
}
