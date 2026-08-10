namespace NovaCore.Inventory.Domain.Enums;

public enum InventoryReservationStatus : byte
{
    Pending = 1,
    Reserved = 2,
    Released = 3,
    Committed = 4,
    Expired = 5,
    Cancelled = 6,
}