namespace NovaCore.Inventory.Application.Features.InventoryLots.DTOs;

public sealed record CreateInventoryLotRequest(
    Guid InventoryId,
    string LotNumber,
    DateTime ManufactureDate,
    DateTime ExpiredDate,
    int Quantity,
    string SupplierLotNumber = "",
    string CountryOfOrigin = "");
