using NovaCore.Inventory.Domain.Entities.Inventories;
using NovaCore.Inventory.Domain.Entities.Warehouses;
using NovaCore.Inventory.Domain.Metadata;

namespace NovaCore.Inventory.Domain.Entities.InventoryReservations;

public sealed class InventoryReservation : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public string Number { get; private set; } = string.Empty;
    public InventoryReservationType Type { get; private set; }
    public InventoryReservationStatus Status { get; private set; }
    public Guid InventoryId { get; private set; }
    public InventoryStock Inventory { get; private set; } = default!;
    public Guid WarehouseId { get; private set; }
    public Warehouse Warehouse { get; private set; } = default!;
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public InventoryReferenceType? ReferenceType { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public string ExternalReference { get; private set; } = string.Empty;
    public Quantity Quantity { get; private set; } = default!;
    public DateTime? ExpiredAt { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public InventoryReservationMetadata Metadata { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private InventoryReservation() { }

    public static InventoryReservation Create(
        string number,
        InventoryReservationType type,
        Guid inventoryId,
        Guid warehouseId,
        Guid productId,
        Guid productVariantId,
        int quantity,
        InventoryReferenceType? referenceType = null,
        Guid? referenceId = null,
        string externalReference = "",
        DateTime? expiredAt = null,
        string reason = "")
    {
        if (number.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Reservation number is required.");

        return new InventoryReservation
        {
            Id = Guid.CreateVersion7(),
            Number = number.Trim(),
            Type = type,
            Status = InventoryReservationStatus.Pending,
            InventoryId = inventoryId,
            WarehouseId = warehouseId,
            ProductId = productId,
            VariantId = productVariantId,
            Quantity = Quantity.Create(quantity),
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            ExternalReference = externalReference,
            ExpiredAt = expiredAt,
            Reason = reason,
            Metadata = InventoryReservationMetadata.Create<InventoryReservationMetadata>(),
        };
    }
    #endregion

    #region Workflow
    public void Confirm()
    {
        if (Status != InventoryReservationStatus.Pending)
            throw ExceptionFactory.InvalidStatus($"Cannot confirm a reservation in {Status} status.");

        Status = InventoryReservationStatus.Reserved;
    }

    public void Commit()
    {
        if (Status != InventoryReservationStatus.Reserved)
            throw ExceptionFactory.InvalidStatus($"Cannot commit a reservation in {Status} status.");

        Status = InventoryReservationStatus.Committed;
    }

    public void Release(string? reason = null)
    {
        if (Status is not (InventoryReservationStatus.Pending or InventoryReservationStatus.Reserved))
            throw ExceptionFactory.InvalidStatus($"Cannot release a reservation in {Status} status.");

        Status = InventoryReservationStatus.Released;
        if (!reason.IsNullOrWhiteSpace())
            Reason = reason!;
    }

    public void Expire()
    {
        if (Status is not (InventoryReservationStatus.Pending or InventoryReservationStatus.Reserved))
            throw ExceptionFactory.InvalidStatus($"Cannot expire a reservation in {Status} status.");

        Status = InventoryReservationStatus.Expired;
    }

    public void Cancel()
    {
        if (Status is InventoryReservationStatus.Committed or InventoryReservationStatus.Cancelled)
            throw ExceptionFactory.InvalidStatus($"Cannot cancel a reservation in {Status} status.");

        Status = InventoryReservationStatus.Cancelled;
    }
    #endregion
}
