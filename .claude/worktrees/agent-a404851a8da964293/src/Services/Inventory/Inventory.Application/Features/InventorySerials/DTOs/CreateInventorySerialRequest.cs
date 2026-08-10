namespace NovaCore.Inventory.Application.Features.InventorySerials.DTOs;

public sealed record CreateInventorySerialRequest(
    Guid InventoryId,
    string SerialNumber);
