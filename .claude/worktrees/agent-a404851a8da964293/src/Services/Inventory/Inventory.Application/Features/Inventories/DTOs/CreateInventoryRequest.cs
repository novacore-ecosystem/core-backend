namespace NovaCore.Inventory.Application.Features.Inventories.DTOs;

public sealed record CreateInventoryRequest(
    Guid ProductId,
    Guid VariationId,
    Guid WarehouseId,
    int Quantity = 0);
