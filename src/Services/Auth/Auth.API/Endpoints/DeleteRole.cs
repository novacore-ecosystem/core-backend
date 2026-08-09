using NovaCore.Auth.Application.Features.Roles.Commands.DeleteRole;

using NovaCore.BuildingBlock.Web.Authorization;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.API.Endpoints;

public sealed class DeleteRole : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/roles/{id:guid}", async (
            Guid id,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new DeleteRoleCommand(id), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Roles")
        .RequirePermissions(Permissions.Role.Manage)
        .WithSummary("Auth_DeleteRole")
        .WithDisplayName("Delete Role API")
        .Produces<ApiResponse<object>>();
    }
}
