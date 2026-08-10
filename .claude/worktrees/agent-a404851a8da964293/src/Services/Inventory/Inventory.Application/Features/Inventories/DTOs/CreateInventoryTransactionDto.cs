namespace NovaCore.Inventory.Application.Features.Inventories.DTOs;

public sealed record CreateInventoryTransactionDto(
    Guid InventoryId,
    Guid ProductId,
    Guid VariantId,
    Guid WarehouseId,
    InventoryTransactionType Type,
    int Quantity,
    int QuantityAfter,
    string Reason);
