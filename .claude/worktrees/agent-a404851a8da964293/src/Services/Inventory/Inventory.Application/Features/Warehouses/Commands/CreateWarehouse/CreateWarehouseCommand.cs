namespace NovaCore.Inventory.Application.Features.Warehouses.Commands.CreateWarehouse;

public sealed record CreateWarehouseCommand(
    string Code,
    string Name,
    WarehouseType Type,
    string Country,
    string StateOrProvince,
    string City,
    string District,
    string Ward,
    string Street,
    string PostalCode) : ICommand<CreateWarehouseResponse>;

public sealed record CreateWarehouseResponse(Guid WarehouseId);
