namespace NovaCore.Inventory.Application.Features.Inventories.Commands.StockOut;

public sealed record StockOutCommand(
    Guid InventoryId,
    int Quantity,
    string Reason) : ICommand<StockOutResponse>;

public sealed record StockOutResponse(int NewQuantity);
