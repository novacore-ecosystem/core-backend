using NovaCore.Inventory.Domain.Entities.Inventories;
using NovaCore.Inventory.Domain.Entities.InventoryDocuments;
using NovaCore.Inventory.Domain.Entities.InventoryReservations;
using NovaCore.Inventory.Domain.Metadata;

namespace NovaCore.Inventory.Domain.Entities.InventorySerials;

public sealed class InventorySerial : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid InventoryId { get; private set; }
    public InventoryStock Inventory { get; private set; } = default!;
    public string SerialNumber { get; private set; } = string.Empty;
    public InventorySerialStatus Status { get; private set; }
    public Guid? InventoryReservationId { get; private set; }
    public InventoryReservation? Reservation { get; private set; }
    public Guid? InventoryDocumentId { get; private set; }
    public InventoryDocument? Document { get; private set; }
    public InventorySerialMetadata Metadata { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private InventorySerial() { }

    public static InventorySerial Create(Guid inventoryId, string serialNumber)
    {
        if (serialNumber.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Serial number is required.");

        return new InventorySerial
        {
            Id = Guid.CreateVersion7(),
            InventoryId = inventoryId,
            SerialNumber = serialNumber.Trim(),
            Status = InventorySerialStatus.Available,
            Metadata = InventorySerialMetadata.Create<InventorySerialMetadata>(),
        };
    }
    #endregion

    #region Workflow
    public void Reserve(Guid reservationId)
    {
        if (Status != InventorySerialStatus.Available)
            throw ExceptionFactory.InvalidStatus($"Cannot reserve a serial in {Status} status.");

        Status = InventorySerialStatus.Reserved;
        InventoryReservationId = reservationId;
    }

    public void ReleaseReservation()
    {
        if (Status != InventorySerialStatus.Reserved)
            throw ExceptionFactory.InvalidStatus($"Cannot release a serial in {Status} status.");

        Status = InventorySerialStatus.Available;
        InventoryReservationId = null;
    }

    public void Sell(Guid documentId)
    {
        if (Status is not (InventorySerialStatus.Available or InventorySerialStatus.Reserved))
            throw ExceptionFactory.InvalidStatus($"Cannot sell a serial in {Status} status.");

        Status = InventorySerialStatus.Sold;
        InventoryDocumentId = documentId;
    }

    public void Return(Guid documentId)
    {
        if (Status != InventorySerialStatus.Sold)
            throw ExceptionFactory.InvalidStatus($"Cannot return a serial in {Status} status.");

        Status = InventorySerialStatus.Returned;
        InventoryDocumentId = documentId;
    }

    public void RestoreToAvailable()
    {
        if (Status != InventorySerialStatus.Returned)
            throw ExceptionFactory.InvalidStatus($"Cannot restore a serial in {Status} status.");

        Status = InventorySerialStatus.Available;
        InventoryReservationId = null;
        InventoryDocumentId = null;
    }

    public void MarkDamaged()
    {
        if (Status is InventorySerialStatus.Sold)
            throw ExceptionFactory.InvalidStatus($"Cannot mark a sold serial as damaged.");

        Status = InventorySerialStatus.Damaged;
    }

    public void MarkLost()
    {
        if (Status is InventorySerialStatus.Sold)
            throw ExceptionFactory.InvalidStatus($"Cannot mark a sold serial as lost.");

        Status = InventorySerialStatus.Lost;
    }
    #endregion
}
