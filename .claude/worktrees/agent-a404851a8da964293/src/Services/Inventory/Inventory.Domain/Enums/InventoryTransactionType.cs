namespace NovaCore.Inventory.Domain.Enums;

public enum InventoryTransactionType : short
{
    Receipt = 1,
    Reservation = 2,
    ReleaseReservation = 3,
    Deduction = 4,
    TransferIn = 5,
    TransferOut = 6,
    Adjustment = 7,
    Return = 8,
    Production = 9,
    Consumption = 10,
    Damage = 11,
    Restore = 12,
    CycleCount = 13,
}
