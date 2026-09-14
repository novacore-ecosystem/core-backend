using NovaCore.Auth.Application.Features.Apps.Commands.UpdateApp;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Apps;

public record UpdateAppRequest(string Name, bool IsActive);

public sealed class UpdateAppEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/apps/{id:guid}", async (
            Guid id,
            [FromBody] UpdateAppRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new UpdateAppCommand(id, request.Name, request.IsActive);
            await sender.Send(command, ct);
            return ApiResponse<object>.Ok();
        })
        .WithTags("Apps")
        .RequirePermissions(Permissions.App.Manage)
        .WithSummary("Auth_UpdateApp")
        .WithDisplayName("Update App API")
        .Produces<ApiResponse<object>>();
    }
}
