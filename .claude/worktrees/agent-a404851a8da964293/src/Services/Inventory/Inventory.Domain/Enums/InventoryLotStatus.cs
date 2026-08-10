namespace NovaCore.Inventory.Domain.Enums;

public enum InventoryLotStatus : byte
{
    Active = 1,
    Quarantined = 2,
    Expired = 3,
    Consumed = 4,
    Archived = 5,
}
