namespace NovaCore.Inventory.Domain.Enums;

public enum InventoryReferenceType : short
{
    Order = 1,
    PurchaseOrder = 2,
    Transfer = 3,
    Adjustment = 4,
    Return = 5,
    CycleCount = 6,
    Manual = 7,
    System = 8,
}
