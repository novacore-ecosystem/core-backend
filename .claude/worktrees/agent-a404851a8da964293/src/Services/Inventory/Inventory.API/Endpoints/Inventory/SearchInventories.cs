using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Inventory.Application.Features.Inventories.Queries.SearchInventories;

namespace NovaCore.Inventory.API.Endpoints.Inventory;

public sealed class SearchInventoriesEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Search Inventory Records",
        "",
        "Admin/Root only. Paginated, filterable, sortable search over stock-keeping rows, backed by Postgres indexes.",
        "",
        "### Request Body",
        "- **filters**: `[{ field, operator, value }]` - no free-text `keyword` support, stock rows have no text field",
        "- **sorts**: `[{ field, direction }]` - direction is `asc`/`desc`",
        "- **page** / **pageSize**: 1-based paging (default 1 / 20)",
        "",
        "### Searchable Fields",
        "- **productId**, **productVariationId**, **warehouseId**: eq/ne/in/nin",
        "- **quantity**: eq/ne/gt/gte/lt/lte/in/nin, sortable",
        "- **createdAt**: eq/ne/gt/gte/lt/lte/between, sortable",
        "",
        "### Error Responses",
        "- **400**: Unknown field, operator not allowed for the field, or malformed value",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/inventories/search", Handle)
            .WithTags("Inventory")
            .RequirePermissions(Permissions.Inventory.View)
            .WithName("SearchInventories")
            .WithDisplayName("Search Inventory Records API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<PaginatedResult<SearchInventoriesItemResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromBody] CriteriaRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new SearchInventoriesQuery(request);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<PaginatedResult<SearchInventoriesItemResponse>>.Ok(response));
    }
}
