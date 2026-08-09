using NovaCore.Auth.Application.Features.Roles.Queries.ListRoles;

using NovaCore.BuildingBlock.Web.Authorization;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.API.Endpoints.Roles;

public sealed class ListRolesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/roles", async (
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var response = await sender.Send(new ListRolesQuery(), ct);
            return ApiResponse<IReadOnlyList<RoleSummaryResponse>>.Ok(response);
        })
        .WithTags("Roles")
        .RequirePermissions(Permissions.Role.View)
        .WithSummary("Auth_ListRoles")
        .WithDisplayName("List Roles API")
        .Produces<ApiResponse<IReadOnlyList<RoleSummaryResponse>>>();
    }
}
