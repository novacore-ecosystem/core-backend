namespace NovaCore.Inventory.Application.Features.Inventories.Commands.TransferInventory;

public sealed record TransferInventoryItem(
    Guid VariantId,
    int Quantity);

public sealed record TransferInventoryCommand(
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    IReadOnlyList<TransferInventoryItem> Items,
    string Reason) : ICommand<TransferInventoryResponse>;

public sealed record TransferInventoryResponse(
    string TransferId,
    int ItemsTransferred,
    int TotalQuantity);
