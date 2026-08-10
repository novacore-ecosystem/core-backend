namespace NovaCore.Inventory.Application.Features.Inventories.Commands.ReceiveInventory;

public sealed record ReceiveInventoryItem(
    Guid VariantId,
    int Quantity,
    string? LotNumber = null,
    DateTime? ManufactureDate = null,
    DateTime? ExpiryDate = null);

public sealed record ReceiveInventoryCommand(
    string PurchaseOrderNumber,
    Guid WarehouseId,
    IReadOnlyList<ReceiveInventoryItem> Items,
    string Description) : ICommand<ReceiveInventoryResponse>;

public sealed record ReceiveInventoryResponse(
    int ItemsReceived,
    int LotsCreated);
