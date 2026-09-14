using NovaCore.Auth.Application.Features.Apps.Queries.ListApps;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.Web.Authorization;

namespace NovaCore.Auth.API.Endpoints.Apps;

public sealed class ListAppsEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## List Apps",
        "",
        "Returns a paginated, searchable App list for the Root Portal's App Management screen.",
        "",
        "### Query Parameters",
        "- **search**: optional, matches App Code or Name (case-insensitive)",
        "- **page**: 1-based page number, default 1",
        "- **pageSize**: items per page, default 20",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/apps", async (
            [FromQuery] string? search,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromServices] ISender sender,
            CancellationToken ct = default) =>
        {
            var query = new ListAppsQuery(
                search,
                page <= 0 ? 1 : page,
                pageSize <= 0 ? 20 : pageSize);
            var response = await sender.Send(query, ct);
            return ApiResponse<PaginatedResult<AppSummaryResponse>>.Ok(response);
        })
        .WithTags("Apps")
        .RequirePermissions(Permissions.App.View)
        .WithSummary("Auth_ListApps")
        .WithDisplayName("List Apps API")
        .WithDescription(API_DESC.JoinToString("\n"))
        .Produces<ApiResponse<PaginatedResult<AppSummaryResponse>>>();
    }
}
