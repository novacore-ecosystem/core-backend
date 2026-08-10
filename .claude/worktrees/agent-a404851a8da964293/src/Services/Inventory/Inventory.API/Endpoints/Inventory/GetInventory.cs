using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Inventory.Application.Features.Inventories.Queries.GetInventory;

namespace NovaCore.Inventory.API.Endpoints.Inventory;

public sealed class GetInventoryEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Inventory Details",
        "",
        "Retrieves current stock quantity for an inventory record.",
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
        app.MapGet("/inventories/{inventoryId}", Handle)
            .WithTags("Inventory")
            .RequireAuthorization()
            .WithName("GetInventory")
            .WithDisplayName("Get Inventory API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetInventoryResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid inventoryId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetInventoryQuery(inventoryId);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetInventoryResponse>.Ok(response));
    }
}
