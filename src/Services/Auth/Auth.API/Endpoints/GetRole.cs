using NovaCore.Auth.Application.Features.Roles.Queries.GetRole;

using NovaCore.BuildingBlock.Web.Authorization;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.API.Endpoints;

public sealed class GetRole : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/roles/{id:guid}", async (
            Guid id,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var response = await sender.Send(new GetRoleQuery(id), ct);
            return ApiResponse<RoleDetailResponse>.Ok(response);
        })
        .WithTags("Roles")
        .RequirePermissions(Permissions.Role.View)
        .WithSummary("Auth_GetRole")
        .WithDisplayName("Get Role API")
        .Produces<ApiResponse<RoleDetailResponse>>();
    }
}
