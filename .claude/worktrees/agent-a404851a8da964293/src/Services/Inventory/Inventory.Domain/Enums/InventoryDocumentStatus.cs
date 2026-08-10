namespace NovaCore.Inventory.Domain.Enums;

public enum InventoryDocumentStatus : byte
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    Completed = 4,
    Cancelled = 5,
}
