using NovaCore.Auth.Application.Features.Permissions.Queries.ListPermissions;

using NovaCore.BuildingBlock.Web.Authorization;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.API.Endpoints;

public sealed class ListPermissions : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/permissions", async (
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var response = await sender.Send(new ListPermissionsQuery(), ct);
            return ApiResponse<IReadOnlyList<PermissionSummaryResponse>>.Ok(response);
        })
        .WithTags("Permissions")
        .RequirePermissions(Permissions.Permission.View)
        .WithSummary("Auth_ListPermissions")
        .WithDisplayName("List Permissions API")
        .Produces<ApiResponse<IReadOnlyList<PermissionSummaryResponse>>>();
    }
}
