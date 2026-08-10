namespace NovaCore.Inventory.Domain.Enums;

public enum InventoryDocumentReason : short
{
    None = 1,
    Purchase = 2,
    Sale = 3,
    CustomerReturn = 4,
    SupplierReturn = 5,
    Transfer = 6,
    Adjustment = 7,
    CycleCount = 8,
    Damaged = 9,
    Expired = 10,
    Lost = 11,
    Production = 12,
    Consumption = 13,
    Manual = 14,
}
