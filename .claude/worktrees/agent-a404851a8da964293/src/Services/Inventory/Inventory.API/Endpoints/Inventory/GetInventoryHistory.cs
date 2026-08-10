using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Inventory.Application.Features.Inventories.Queries.GetInventoryHistory;

namespace NovaCore.Inventory.API.Endpoints.Inventory;

public sealed class GetInventoryHistoryEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Inventory History",
        "",
        "Retrieves the stock movement history (stock in / stock out / adjustments) for an inventory record.",
        "",
        "### Route Parameters",
        "- **inventoryId**: Unique identifier of the inventory record (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Inventory not found",
        "- **400**: Invalid inventoryId format",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/inventories/{inventoryId}/history", Handle)
            .WithTags("Inventory")
            .RequirePermissions(Permissions.Inventory.View)
            .WithName("GetInventoryHistory")
            .WithDisplayName("Get Inventory History API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetInventoryHistoryResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid inventoryId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetInventoryHistoryQuery(inventoryId);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetInventoryHistoryResponse>.Ok(response));
    }
}
