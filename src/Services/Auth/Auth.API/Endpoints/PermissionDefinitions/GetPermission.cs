using NovaCore.Auth.Application.Features.Permissions.Queries.GetPermission;

using NovaCore.BuildingBlock.Web.Authorization;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.API.Endpoints.PermissionDefinitions;

public sealed class GetPermissionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/permissions/{id:guid}", async (
            Guid id,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var response = await sender.Send(new GetPermissionQuery(id), ct);
            return ApiResponse<PermissionDetailResponse>.Ok(response);
        })
        .WithTags("Permissions")
        .RequirePermissions(Permissions.Permission.View)
        .WithSummary("Auth_GetPermission")
        .WithDisplayName("Get Permission API")
        .Produces<ApiResponse<PermissionDetailResponse>>();
    }
}
