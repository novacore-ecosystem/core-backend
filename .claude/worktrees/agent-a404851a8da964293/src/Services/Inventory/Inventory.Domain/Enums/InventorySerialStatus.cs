namespace NovaCore.Inventory.Domain.Enums;

public enum InventorySerialStatus : byte
{
    Available = 1,
    Reserved = 2,
    Sold = 3,
    Returned = 4,
    Damaged = 5,
    Lost = 6,
}
