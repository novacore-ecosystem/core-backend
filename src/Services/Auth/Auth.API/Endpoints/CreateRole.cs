using NovaCore.Auth.Application.Features.Roles.Commands.CreateRole;

using NovaCore.BuildingBlock.Web.Authorization;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.API.Endpoints;

public record CreateRoleRequest(string Name, string Code, string? Description);

public sealed class CreateRole : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/roles", async (
            [FromBody] CreateRoleRequest request,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var id = await sender.Send(new CreateRoleCommand(request.Name, request.Code, request.Description), ct);
            return ApiResponse<Guid>.Ok(id);
        })
        .WithTags("Roles")
        .RequirePermissions(Permissions.Role.Manage)
        .WithSummary("Auth_CreateRole")
        .WithDisplayName("Create Role API")
        .Produces<ApiResponse<Guid>>();
    }
}
