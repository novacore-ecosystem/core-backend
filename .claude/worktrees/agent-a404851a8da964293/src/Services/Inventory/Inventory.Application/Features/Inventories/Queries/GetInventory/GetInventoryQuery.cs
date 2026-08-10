namespace NovaCore.Inventory.Application.Features.Inventories.Queries.GetInventory;

public sealed record GetInventoryQuery(Guid InventoryId) : IQuery<GetInventoryResponse>;

public sealed record GetInventoryResponse(
    Guid Id,
    Guid ProductId,
    Guid VariantId,
    Guid WarehouseId,
    int Quantity,
    DateTime CreatedAt,
    DateTime UpdatedAt);
