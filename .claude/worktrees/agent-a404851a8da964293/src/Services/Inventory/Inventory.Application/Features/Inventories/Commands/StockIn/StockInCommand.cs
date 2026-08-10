namespace NovaCore.Inventory.Application.Features.Inventories.Commands.StockIn;

public sealed record StockInCommand(
    Guid InventoryId,
    int Quantity,
    string Reason) : ICommand<StockInResponse>;

public sealed record StockInResponse(int NewQuantity);
