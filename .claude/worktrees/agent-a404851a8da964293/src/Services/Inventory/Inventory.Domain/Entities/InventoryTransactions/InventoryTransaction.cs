using NovaCore.Inventory.Domain.Entities.Inventories;
using NovaCore.Inventory.Domain.Entities.InventoryDocuments;
using NovaCore.Inventory.Domain.Entities.InventoryReservations;
using NovaCore.Inventory.Domain.Entities.Warehouses;
using NovaCore.Inventory.Domain.Metadata;

namespace NovaCore.Inventory.Domain.Entities.InventoryTransactions;

/// <summary>
/// Immutable ledger entry recording a single stock movement - never mutated after creation, only
/// ever appended to (same idea as OrderDiscount's applied amounts being append-only history).
/// </summary>
public sealed class InventoryTransaction : AggregateRoot<Guid>, ITenantEntity
{
    public Guid InventoryId { get; private set; }
    public InventoryStock Inventory { get; private set; } = default!;
    public Guid WarehouseId { get; private set; }
    public Warehouse Warehouse { get; private set; } = default!;
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public Guid? InventoryDocumentId { get; private set; }
    public InventoryDocument? Document { get; private set; }
    public Guid? InventoryReservationId { get; private set; }
    public InventoryReservation? Reservation { get; private set; }
    public InventoryTransactionType Type { get; private set; }
    public Quantity Quantity { get; private set; } = default!;
    public int BeforeOnHandQuantity { get; private set; }
    public int AfterOnHandQuantity { get; private set; }
    public int BeforeReservedQuantity { get; private set; }
    public int AfterReservedQuantity { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public InventoryTransactionMetadata Metadata { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private InventoryTransaction() { }

    public static InventoryTransaction Create(
        Guid inventoryId,
        Guid warehouseId,
        Guid productId,
        Guid variantId,
        InventoryTransactionType type,
        int quantity,
        int beforeOnHandQuantity,
        int afterOnHandQuantity,
        int beforeReservedQuantity,
        int afterReservedQuantity,
        string description = "",
        Guid? inventoryDocumentId = null,
        Guid? inventoryReservationId = null)
    {
        if (quantity <= 0)
            throw ExceptionFactory.InvalidRange("Transaction quantity must be greater than 0.");

        return new InventoryTransaction
        {
            Id = Guid.CreateVersion7(),
            InventoryId = inventoryId,
            WarehouseId = warehouseId,
            ProductId = productId,
            VariantId = variantId,
            Type = type,
            Quantity = Quantity.Create(quantity),
            BeforeOnHandQuantity = beforeOnHandQuantity,
            AfterOnHandQuantity = afterOnHandQuantity,
            BeforeReservedQuantity = beforeReservedQuantity,
            AfterReservedQuantity = afterReservedQuantity,
            Description = description,
            InventoryDocumentId = inventoryDocumentId,
            InventoryReservationId = inventoryReservationId,
            Metadata = InventoryTransactionMetadata.Create<InventoryTransactionMetadata>(),
        };
    }
}
