using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Inventory.Application.Features.Warehouses.Queries.SearchWarehouses;

namespace NovaCore.Inventory.API.Endpoints.Warehouse;

public sealed class SearchWarehousesEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Search Warehouses",
        "",
        "Admin/Root only. Paginated, filterable, sortable search over warehouses, backed by Postgres indexes.",
        "",
        "### Request Body",
        "- **keyword**: Free-text match against code/name/address (optional)",
        "- **filters**: `[{ field, operator, value }]`",
        "- **sorts**: `[{ field, direction }]` - direction is `asc`/`desc`",
        "- **page** / **pageSize**: 1-based paging (default 1 / 20)",
        "",
        "### Searchable Fields",
        "- **code**, **name**: eq/ne/c/sw/ew/in/nin, sortable, included in keyword search",
        "- **address**: eq/ne/c/sw/ew/in/nin, included in keyword search",
        "- **status**: eq/ne/in/nin, sortable",
        "- **createdAt**: eq/ne/gt/gte/lt/lte/between, sortable",
        "",
        "### Error Responses",
        "- **400**: Unknown field, operator not allowed for the field, or malformed value",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/warehouses/search", Handle)
            .WithTags("Warehouse")
            .RequirePermissions(Permissions.Warehouse.View)
            .WithName("SearchWarehouses")
            .WithDisplayName("Search Warehouses API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<PaginatedResult<SearchWarehousesItemResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromBody] CriteriaRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new SearchWarehousesQuery(request);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<PaginatedResult<SearchWarehousesItemResponse>>.Ok(response));
    }
}
