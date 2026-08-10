using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Inventory.Application.Features.Inventories.Commands.ReceiveInventory;

namespace NovaCore.Inventory.API.Endpoints.Inventory;

public sealed record ReceiveInventoryItemRequest(
    string VariantId,
    int Quantity,
    string? LotNumber = null,
    DateTime? ManufactureDate = null,
    DateTime? ExpiryDate = null);

public sealed record ReceiveInventoryRequest(
    string PurchaseOrderNumber,
    string WarehouseId,
    IReadOnlyList<ReceiveInventoryItemRequest> Items,
    string Description);

public sealed class ReceiveInventoryEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Receive Inventory",
        "",
        "Records receipt of inventory items into a warehouse (typically from purchase order).",
        "Supports lot tracking for lot-tracked products.",
        "",
        "### Request Body",
        "- **PurchaseOrderNumber**: Reference PO number (required)",
        "- **WarehouseId**: Destination warehouse ID (required, must be valid GUID)",
        "- **Items**: Array of receiving items (required, minimum 1 item)",
        "  - **VariantId**: Variant being received (required, must be valid GUID)",
        "  - **Quantity**: Amount received (required, must be > 0)",
        "  - **LotNumber**: Lot number for lot-tracked items (optional)",
        "  - **ManufactureDate**: Lot manufacture date (optional)",
        "  - **ExpiryDate**: Lot expiry date (optional)",
        "- **Description**: Receiving notes (required)",
        "",
        "### Response",
        "- **ItemsReceived**: Number of unique items received",
        "- **LotsCreated**: Number of lot records created",
        "",
        "### Error Responses",
        "- **404**: Warehouse or inventory not found",
        "- **400**: Invalid request or validation failed",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/inventories/receive", Handle)
            .WithTags("Inventory")
            .RequirePermissions(Permissions.Inventory.Receive)
            .WithName("ReceiveInventory")
            .WithDisplayName("Receive Inventory API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<ReceiveInventoryResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromBody] ReceiveInventoryRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var items = request.Items
            .Select(i => new ReceiveInventoryItem(
                VariantId: Guid.Parse(i.VariantId),
                Quantity: i.Quantity,
                LotNumber: i.LotNumber,
                ManufactureDate: i.ManufactureDate,
                ExpiryDate: i.ExpiryDate))
            .ToList();

        var command = new ReceiveInventoryCommand(
            request.PurchaseOrderNumber,
            Guid.Parse(request.WarehouseId),
            items,
            request.Description.Trim());

        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<ReceiveInventoryResponse>.Ok(response));
    }
}
