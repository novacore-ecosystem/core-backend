using NovaCore.Auth.Application.Features.Apps.Commands.CreateApp;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Apps;

public record CreateAppRequest(string Code, string Name);

public sealed class CreateAppEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/apps", async (
            [FromBody] CreateAppRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new CreateAppCommand(request.Code, request.Name);
            var id = await sender.Send(command, ct);
            return ApiResponse<Guid>.Ok(id);
        })
        .WithTags("Apps")
        .RequirePermissions(Permissions.App.Manage)
        .WithSummary("Auth_CreateApp")
        .WithDisplayName("Create App API")
        .Produces<ApiResponse<Guid>>();
    }
}
