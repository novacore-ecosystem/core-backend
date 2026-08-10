using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Inventory.Application.Features.Inventories.Queries.SearchInventoryTransactions;

namespace NovaCore.Inventory.API.Endpoints.Inventory;

public sealed class SearchInventoryTransactionsEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Search Inventory Transactions",
        "",
        "Admin/Root only. Paginated, filterable, sortable search over stock movements across every",
        "inventory row - unlike `GET /inventories/{inventoryId}/history`, not scoped to a single inventory id.",
        "",
        "### Request Body",
        "- **keyword**: Free-text match against reason (optional)",
        "- **filters**: `[{ field, operator, value }]`",
        "- **sorts**: `[{ field, direction }]` - direction is `asc`/`desc`",
        "- **page** / **pageSize**: 1-based paging (default 1 / 20)",
        "",
        "### Searchable Fields",
        "- **inventoryId**, **productId**, **productVariationId**, **warehouseId**: eq/ne/in/nin",
        "- **type**: eq/ne/in/nin, sortable",
        "- **quantity**, **quantityAfter**: eq/ne/gt/gte/lt/lte/in/nin, sortable",
        "- **reason**: eq/ne/c/sw/ew/in/nin, included in keyword search",
        "- **createdAt**: eq/ne/gt/gte/lt/lte/between, sortable",
        "",
        "### Error Responses",
        "- **400**: Unknown field, operator not allowed for the field, or malformed value",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/inventory-transactions/search", Handle)
            .WithTags("Inventory")
            .RequirePermissions(Permissions.Inventory.View)
            .WithName("SearchInventoryTransactions")
            .WithDisplayName("Search Inventory Transactions API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<PaginatedResult<SearchInventoryTransactionsItemResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromBody] CriteriaRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new SearchInventoryTransactionsQuery(request);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<PaginatedResult<SearchInventoryTransactionsItemResponse>>.Ok(response));
    }
}
