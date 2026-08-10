using NovaCore.Inventory.Domain.Entities.Warehouses;
using NovaCore.Inventory.Domain.Metadata;

namespace NovaCore.Inventory.Domain.Entities.Inventories;

/// <summary>Per-warehouse stock level for a single product variant.</summary>
public sealed class InventoryStock : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid WarehouseId { get; private set; }
    public Warehouse Warehouse { get; private set; } = default!;
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public Sku Sku { get; private set; } = default!;
    public InventoryStatus Status { get; private set; }
    public Quantity OnHandQuantity { get; private set; } = default!;
    public Quantity ReservedQuantity { get; private set; } = default!;
    public Quantity IncomingQuantity { get; private set; } = default!;
    public Quantity OutgoingQuantity { get; private set; } = default!;
    public Quantity DamagedQuantity { get; private set; } = default!;
    public Quantity SafetyStock { get; private set; } = default!;
    public Quantity ReorderPoint { get; private set; } = default!;
    public Quantity MaximumStock { get; private set; } = default!;
    /// <summary>Sellable stock - derived, never stored directly (OnHand minus what's already promised).</summary>
    public int AvailableQuantity => OnHandQuantity.Value - ReservedQuantity.Value;
    public InventoryMetadata Metadata { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private InventoryStock() { }

    public static InventoryStock Create(
        Guid productId,
        Guid variantId,
        Guid warehouseId,
        Sku sku,
        int safetyStock = 0,
        int reorderPoint = 0,
        int maximumStock = 0)
    {
        return new InventoryStock
        {
            Id = Guid.CreateVersion7(),
            ProductId = productId,
            VariantId = variantId,
            WarehouseId = warehouseId,
            Sku = sku,
            Status = InventoryStatus.Active,
            OnHandQuantity = Quantity.Create(0),
            ReservedQuantity = Quantity.Create(0),
            IncomingQuantity = Quantity.Create(0),
            OutgoingQuantity = Quantity.Create(0),
            DamagedQuantity = Quantity.Create(0),
            SafetyStock = Quantity.Create(safetyStock),
            ReorderPoint = Quantity.Create(reorderPoint),
            MaximumStock = Quantity.Create(maximumStock),
            Metadata = InventoryMetadata.Create<InventoryMetadata>(),
        };
    }
    #endregion

    #region Stock movement
    /// <summary>Increases on-hand stock, e.g. from a completed receipt.</summary>
    public void ReceiveStock(int quantity)
    {
        if (quantity <= 0)
            throw ExceptionFactory.InvalidRange("Quantity to receive must be greater than 0.");

        OnHandQuantity = Quantity.Create(OnHandQuantity.Value + quantity);
    }

    /// <summary>Deducts on-hand stock directly, without going through a reservation.</summary>
    public void DeductStock(int quantity)
    {
        if (quantity <= 0)
            throw ExceptionFactory.InvalidRange("Quantity to deduct must be greater than 0.");

        if (quantity > AvailableQuantity)
            throw ExceptionFactory.InsufficientStock(
                $"Insufficient stock: available {AvailableQuantity}, required {quantity}.");

        OnHandQuantity = Quantity.Create(OnHandQuantity.Value - quantity);
    }

    /// <summary>Directly corrects on-hand stock to a known-good value, e.g. after a physical count.</summary>
    public void AdjustStock(int newOnHandQuantity)
    {
        OnHandQuantity = Quantity.Create(newOnHandQuantity);
    }

    public void MarkDamaged(int quantity)
    {
        if (quantity <= 0)
            throw ExceptionFactory.InvalidRange("Quantity to mark as damaged must be greater than 0.");

        if (quantity > AvailableQuantity)
            throw ExceptionFactory.InsufficientStock(
                $"Insufficient stock: available {AvailableQuantity}, required {quantity}.");

        OnHandQuantity = Quantity.Create(OnHandQuantity.Value - quantity);
        DamagedQuantity = Quantity.Create(DamagedQuantity.Value + quantity);
    }

    public void RestoreDamaged(int quantity)
    {
        if (quantity <= 0)
            throw ExceptionFactory.InvalidRange("Quantity to restore must be greater than 0.");

        if (quantity > DamagedQuantity.Value)
            throw ExceptionFactory.InvalidRange("Cannot restore more than the damaged quantity on hand.");

        DamagedQuantity = Quantity.Create(DamagedQuantity.Value - quantity);
        OnHandQuantity = Quantity.Create(OnHandQuantity.Value + quantity);
    }
    #endregion

    #region Reservation
    public void Reserve(int quantity)
    {
        if (quantity <= 0)
            throw ExceptionFactory.InvalidRange("Quantity to reserve must be greater than 0.");

        if (quantity > AvailableQuantity)
            throw ExceptionFactory.InsufficientStock(
                $"Insufficient stock: available {AvailableQuantity}, required {quantity}.");

        ReservedQuantity = Quantity.Create(ReservedQuantity.Value + quantity);
    }

    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0)
            throw ExceptionFactory.InvalidRange("Quantity to release must be greater than 0.");

        if (quantity > ReservedQuantity.Value)
            throw ExceptionFactory.InvalidRange("Cannot release more than the reserved quantity.");

        ReservedQuantity = Quantity.Create(ReservedQuantity.Value - quantity);
    }

    /// <summary>Fulfils a reservation - moves stock out of both Reserved and OnHand.</summary>
    public void CommitReservation(int quantity)
    {
        if (quantity <= 0)
            throw ExceptionFactory.InvalidRange("Quantity to commit must be greater than 0.");

        if (quantity > ReservedQuantity.Value)
            throw ExceptionFactory.InvalidRange("Cannot commit more than the reserved quantity.");

        ReservedQuantity = Quantity.Create(ReservedQuantity.Value - quantity);
        OnHandQuantity = Quantity.Create(OnHandQuantity.Value - quantity);
    }
    #endregion

    #region Thresholds & status
    public void UpdateThresholds(int safetyStock, int reorderPoint, int maximumStock)
    {
        SafetyStock = Quantity.Create(safetyStock);
        ReorderPoint = Quantity.Create(reorderPoint);
        MaximumStock = Quantity.Create(maximumStock);
    }

    public void Activate()
    {
        Status = InventoryStatus.Active;
    }

    public void Deactivate()
    {
        Status = InventoryStatus.Inactive;
    }

    public void Block()
    {
        Status = InventoryStatus.Blocked;
    }

    public void Archive()
    {
        Status = InventoryStatus.Archived;
    }
    #endregion
}
