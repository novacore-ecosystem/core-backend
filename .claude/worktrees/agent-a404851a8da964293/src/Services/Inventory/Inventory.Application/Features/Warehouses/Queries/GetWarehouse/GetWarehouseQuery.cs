namespace NovaCore.Inventory.Application.Features.Warehouses.Queries.GetWarehouse;

public sealed record GetWarehouseQuery(Guid WarehouseId) : IQuery<GetWarehouseResponse>;

public sealed record GetWarehouseResponse(
    Guid Id,
    string Code,
    string Name,
    string Address,
    WarehouseStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
