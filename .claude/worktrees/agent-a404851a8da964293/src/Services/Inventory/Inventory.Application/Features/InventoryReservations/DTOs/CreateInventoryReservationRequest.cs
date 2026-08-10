namespace NovaCore.Inventory.Application.Features.InventoryReservations.DTOs;

public sealed record CreateInventoryReservationRequest(
    string Number,
    InventoryReservationType Type,
    Guid InventoryId,
    Guid WarehouseId,
    Guid ProductId,
    Guid VariantId,
    int Quantity,
    InventoryReferenceType? ReferenceType = null,
    Guid? ReferenceId = null,
    string ExternalReference = "",
    DateTime? ExpiredAt = null,
    string Reason = "");
