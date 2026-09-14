using NovaCore.Auth.Application.Features.Apps.Commands.DeleteApp;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Apps;

public sealed class DeleteAppEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/apps/{id:guid}", async (
            Guid id,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            await sender.Send(new DeleteAppCommand(id), ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Apps")
        .RequirePermissions(Permissions.App.Manage)
        .WithSummary("Auth_DeleteApp")
        .WithDisplayName("Delete App API")
        .Produces<ApiResponse<object>>();
    }
}
