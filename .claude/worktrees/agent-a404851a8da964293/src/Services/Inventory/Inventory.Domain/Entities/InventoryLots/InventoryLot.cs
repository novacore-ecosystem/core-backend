using NovaCore.Inventory.Domain.Entities.Inventories;
using NovaCore.Inventory.Domain.Metadata;

namespace NovaCore.Inventory.Domain.Entities.InventoryLots;

public sealed class InventoryLot : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid InventoryId { get; private set; }
    public InventoryStock Inventory { get; private set; } = default!;
    public string LotNumber { get; private set; } = string.Empty;
    public DateTime ManufactureDate { get; private set; }
    public DateTime ExpiredDate { get; private set; }
    public string SupplierLotNumber { get; private set; } = string.Empty;
    public string CountryOfOrigin { get; private set; } = string.Empty;
    public InventoryLotStatus Status { get; private set; }
    public Quantity Quantity { get; private set; } = default!;
    public InventoryLotMetadata Metadata { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private InventoryLot() { }

    public static InventoryLot Create(
        Guid inventoryId,
        string lotNumber,
        DateTime manufactureDate,
        DateTime expiredDate,
        int quantity,
        string supplierLotNumber = "",
        string countryOfOrigin = "")
    {
        if (lotNumber.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Lot number is required.");

        if (expiredDate <= manufactureDate)
            throw ExceptionFactory.InvalidRange("Expired date must be after the manufacture date.");

        return new InventoryLot
        {
            Id = Guid.CreateVersion7(),
            InventoryId = inventoryId,
            LotNumber = lotNumber.Trim(),
            ManufactureDate = manufactureDate,
            ExpiredDate = expiredDate,
            SupplierLotNumber = supplierLotNumber,
            CountryOfOrigin = countryOfOrigin,
            Status = InventoryLotStatus.Active,
            Quantity = Quantity.Create(quantity),
            Metadata = InventoryLotMetadata.Create<InventoryLotMetadata>(),
        };
    }
    #endregion

    #region Stock movement
    public void Consume(int quantity)
    {
        if (Status != InventoryLotStatus.Active)
            throw ExceptionFactory.InvalidStatus($"Cannot consume a lot in {Status} status.");

        if (quantity <= 0)
            throw ExceptionFactory.InvalidRange("Quantity to consume must be greater than 0.");

        if (quantity > Quantity.Value)
            throw ExceptionFactory.InsufficientStock(
                $"Insufficient lot quantity: available {Quantity.Value}, required {quantity}.");

        Quantity = Quantity.Create(Quantity.Value - quantity);

        if (Quantity.Value == 0)
            Status = InventoryLotStatus.Consumed;
    }
    #endregion

    #region Status
    public void Quarantine()
    {
        if (Status != InventoryLotStatus.Active)
            throw ExceptionFactory.InvalidStatus($"Cannot quarantine a lot in {Status} status.");

        Status = InventoryLotStatus.Quarantined;
    }

    public void Reactivate()
    {
        if (Status != InventoryLotStatus.Quarantined)
            throw ExceptionFactory.InvalidStatus($"Cannot reactivate a lot in {Status} status.");

        Status = InventoryLotStatus.Active;
    }

    public void MarkExpired()
    {
        if (Status is InventoryLotStatus.Consumed or InventoryLotStatus.Archived)
            throw ExceptionFactory.InvalidStatus($"Cannot expire a lot in {Status} status.");

        Status = InventoryLotStatus.Expired;
    }

    public void Archive()
    {
        Status = InventoryLotStatus.Archived;
    }
    #endregion
}
