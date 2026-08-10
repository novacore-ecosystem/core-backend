using NovaCore.Inventory.Domain.Entities.Warehouses;

namespace NovaCore.Inventory.Domain.Entities.InventoryCounts;

public sealed class InventoryCount : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public string Number { get; private set; } = string.Empty;
    public Guid WarehouseId { get; private set; }
    public Warehouse Warehouse { get; private set; } = default!;
    public InventoryCountStatus Status { get; private set; }
    public DateTime CountDate { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public ICollection<InventoryCountItem> Items { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private InventoryCount() { }

    public static InventoryCount Create(
        string number,
        Guid warehouseId,
        DateTime countDate,
        string description = "")
    {
        if (number.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Count number is required.");

        return new InventoryCount
        {
            Id = Guid.CreateVersion7(),
            Number = number.Trim(),
            WarehouseId = warehouseId,
            Status = InventoryCountStatus.Draft,
            CountDate = countDate,
            Description = description,
        };
    }
    #endregion

    #region Items
    /// <summary>Only InventoryCount may construct/mutate its Items - same reasoning Order applies to OrderItem.</summary>
    public InventoryCountItem AddItem(Guid inventoryId, Guid productVariantId, int expectedQuantity)
    {
        if (Status is not (InventoryCountStatus.Draft or InventoryCountStatus.Counting))
            throw ExceptionFactory.InvalidStatus($"Cannot add items to a count in {Status} status.");

        var item = InventoryCountItem.Create(Id, inventoryId, productVariantId, expectedQuantity);

        Items.Add(item);

        return item;
    }

    public void RecordCount(long itemId, int actualQuantity)
    {
        if (Status != InventoryCountStatus.Counting)
            throw ExceptionFactory.InvalidStatus($"Cannot record a count while the count is in {Status} status.");

        var item = Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw ExceptionFactory.EntityNotFound<InventoryCountItem>(itemId);

        item.RecordActual(actualQuantity);
    }
    #endregion

    #region Workflow
    public void StartCounting()
    {
        if (Status != InventoryCountStatus.Draft)
            throw ExceptionFactory.InvalidStatus($"Cannot start counting a count in {Status} status.");

        if (Items.Count == 0)
            throw ExceptionFactory.EmptyCollection("A count must contain at least one item.");

        Status = InventoryCountStatus.Counting;
    }

    public void SubmitForApproval()
    {
        if (Status != InventoryCountStatus.Counting)
            throw ExceptionFactory.InvalidStatus($"Cannot submit a count in {Status} status.");

        Status = InventoryCountStatus.PendingApproval;
    }

    public void Approve(Guid approvedBy)
    {
        if (Status != InventoryCountStatus.PendingApproval)
            throw ExceptionFactory.InvalidStatus($"Cannot approve a count in {Status} status.");

        Status = InventoryCountStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != InventoryCountStatus.Approved)
            throw ExceptionFactory.InvalidStatus($"Cannot complete a count in {Status} status.");

        Status = InventoryCountStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == InventoryCountStatus.Completed)
            throw ExceptionFactory.InvalidStatus("Cannot cancel a completed count.");

        if (Status == InventoryCountStatus.Cancelled)
            throw ExceptionFactory.InvalidStatus("Count is already cancelled.");

        Status = InventoryCountStatus.Cancelled;
    }
    #endregion
}
