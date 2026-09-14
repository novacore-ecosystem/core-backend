using NovaCore.Auth.Application.Features.Apps.Queries.GetApp;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Apps;

public sealed class GetAppEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/apps/{id:guid}", async (
            Guid id,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var response = await sender.Send(new GetAppQuery(id), ct);
            return ApiResponse<AppDetailResponse>.Ok(response);
        })
        .WithTags("Apps")
        .RequirePermissions(Permissions.App.View)
        .WithSummary("Auth_GetApp")
        .WithDisplayName("Get App API")
        .Produces<ApiResponse<AppDetailResponse>>();
    }
}
