namespace NovaCore.Inventory.Domain.Enums;

public enum InventoryCountStatus : short
{
    Draft = 1,
    Counting = 2,
    PendingApproval = 3,
    Approved = 4,
    Completed = 5,
    Cancelled = 6,
}
