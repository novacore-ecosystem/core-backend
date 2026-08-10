namespace NovaCore.Inventory.Application.Features.Warehouses.DTOs;

public sealed record CreateWarehouseRequest(
    string Code,
    string Name,
    WarehouseType Type,
    string Country,
    string StateOrProvince,
    string City,
    string District,
    string Ward,
    string Street,
    string PostalCode);
