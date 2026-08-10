namespace NovaCore.Inventory.Application.Features.Inventories.Commands.AdjustStock;

public sealed record AdjustStockCommand(
    Guid InventoryId,
    int NewQuantity,
    string Reason) : ICommand<AdjustStockResponse>;

public sealed record AdjustStockResponse(int NewQuantity);
