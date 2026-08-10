using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Inventory.Application.Features.Inventories.Commands.TransferInventory;

namespace NovaCore.Inventory.API.Endpoints.Inventory;

public sealed record TransferInventoryItemRequest(
    string VariantId,
    int Quantity);

public sealed record TransferInventoryRequest(
    string SourceWarehouseId,
    string DestinationWarehouseId,
    IReadOnlyList<TransferInventoryItemRequest> Items,
    string Reason);

public sealed class TransferInventoryEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Transfer Inventory Between Warehouses",
        "",
        "Transfers stock from one warehouse to another atomically.",
        "Creates transfer documents for both source and destination warehouses.",
        "All items are transferred in a single transaction - either all succeed or all fail.",
        "",
        "### Request Body",
        "- **SourceWarehouseId**: Warehouse to transfer from (required, must be valid GUID)",
        "- **DestinationWarehouseId**: Warehouse to transfer to (required, must be valid GUID)",
        "- **Items**: Array of items to transfer (required, minimum 1 item)",
        "  - **VariantId**: Variant being transferred (required, must be valid GUID)",
        "  - **Quantity**: Amount to transfer (required, must be > 0)",
        "- **Reason**: Transfer reason/notes (required, max 500 chars)",
        "",
        "### Response",
        "- **TransferId**: Unique transfer identifier",
        "- **ItemsTransferred**: Number of unique items transferred",
        "- **TotalQuantity**: Total units transferred",
        "",
        "### Error Responses",
        "- **404**: Source or destination warehouse not found, or inventory not found",
        "- **400**: Invalid request, insufficient stock, or validation failed",
        "- **409**: Concurrent update conflict (will auto-retry)",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/inventories/transfer", Handle)
            .WithTags("Inventory")
            .RequirePermissions(Permissions.Inventory.Transfer)
            .WithName("TransferInventory")
            .WithDisplayName("Transfer Inventory API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<TransferInventoryResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromBody] TransferInventoryRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var items = request.Items
            .Select(i => new TransferInventoryItem(
                VariantId: Guid.Parse(i.VariantId),
                Quantity: i.Quantity))
            .ToList();

        var command = new TransferInventoryCommand(
            SourceWarehouseId: Guid.Parse(request.SourceWarehouseId),
            DestinationWarehouseId: Guid.Parse(request.DestinationWarehouseId),
            Items: items,
            Reason: request.Reason.Trim());

        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<TransferInventoryResponse>.Ok(response));
    }
}
