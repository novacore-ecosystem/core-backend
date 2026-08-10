using NovaCore.Inventory.Domain.Entities.InventoryDocuments.Data;
using NovaCore.Inventory.Domain.Entities.Warehouses;
using NovaCore.Inventory.Domain.Metadata;

namespace NovaCore.Inventory.Domain.Entities.InventoryDocuments;

public sealed class InventoryDocument : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public string Number { get; private set; } = string.Empty;
    public InventoryDocumentType Type { get; private set; }
    public InventoryDocumentReason Reason { get; private set; }
    public InventoryDocumentStatus Status { get; private set; }
    public Guid? SourceWarehouseId { get; private set; }
    public Warehouse? SourceWarehouse { get; private set; }
    public Guid? DestinationWarehouseId { get; private set; }
    public Warehouse? DestinationWarehouse { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public InventoryDocumentMetadata Metadata { get; private set; } = default!;
    public ICollection<InventoryDocumentItem> Items { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private InventoryDocument() { }

    /// <summary>Creates a document with auto-generated number using current numbering strategy.</summary>
    public static InventoryDocument Create(
        InventoryDocumentType type,
        InventoryDocumentReason reason,
        Guid? sourceWarehouseId,
        Guid? destinationWarehouseId,
        string description = "")
        => Create(
            DocumentNumber.Generate().Value,
            type,
            reason,
            sourceWarehouseId,
            destinationWarehouseId,
            description);

    /// <summary>Creates a document with an explicit number (e.g. from external system or idempotency key).</summary>
    public static InventoryDocument Create(
        string number,
        InventoryDocumentType type,
        InventoryDocumentReason reason,
        Guid? sourceWarehouseId,
        Guid? destinationWarehouseId,
        string description = "")
    {
        if (number.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Document number is required.");

        ValidateWarehouses(type, sourceWarehouseId, destinationWarehouseId);

        return new InventoryDocument
        {
            Id = Guid.CreateVersion7(),
            Number = number.Trim(),
            Type = type,
            Reason = reason,
            Status = InventoryDocumentStatus.Draft,
            SourceWarehouseId = sourceWarehouseId,
            DestinationWarehouseId = destinationWarehouseId,
            Description = description,
            Metadata = InventoryDocumentMetadata.Create<InventoryDocumentMetadata>(),
        };
    }

    /// <summary>Builds the document plus its Items in one call - see CreateInventoryDocumentData's own remarks.</summary>
    public static InventoryDocument Create(CreateInventoryDocumentData data)
    {
        var document = Create(
            data.Number,
            data.Type,
            data.Reason,
            data.SourceWarehouseId,
            data.DestinationWarehouseId,
            data.Description);

        foreach (var item in data.Items)
        {
            document.AddItem(
                item.ProductId,
                item.VariantId,
                item.Quantity,
                item.UnitOfMeasure,
                item.InventoryId,
                item.InventoryLotId,
                item.InventorySerialId,
                item.Description);
        }

        return document;
    }

    private static void ValidateWarehouses(InventoryDocumentType type, Guid? sourceWarehouseId, Guid? destinationWarehouseId)
    {
        switch (type)
        {
            case InventoryDocumentType.Receipt:
                if (destinationWarehouseId is null)
                    throw ExceptionFactory.RequiredField("A receipt document requires a destination warehouse.");
                break;
            case InventoryDocumentType.Issue:
                if (sourceWarehouseId is null)
                    throw ExceptionFactory.RequiredField("An issue document requires a source warehouse.");
                break;
            case InventoryDocumentType.Transfer:
                if (sourceWarehouseId is null || destinationWarehouseId is null)
                    throw ExceptionFactory.RequiredField("A transfer document requires both a source and destination warehouse.");
                if (sourceWarehouseId == destinationWarehouseId)
                    throw ExceptionFactory.InvalidRange("Source and destination warehouse must differ for a transfer.");
                break;
            default:
                if (sourceWarehouseId is null && destinationWarehouseId is null)
                    throw ExceptionFactory.RequiredField("At least one warehouse (source or destination) is required.");
                break;
        }
    }
    #endregion

    #region Items
    /// <summary>Only InventoryDocument may construct/mutate its Items - same reasoning Order applies to OrderItem.</summary>
    public InventoryDocumentItem AddItem(
        Guid productId,
        Guid productVariantId,
        int quantity,
        string unitOfMeasure,
        Guid? inventoryId = null,
        Guid? inventoryLotId = null,
        Guid? inventorySerialId = null,
        string description = "")
    {
        if (Status != InventoryDocumentStatus.Draft)
            throw ExceptionFactory.InvalidStatus($"Cannot add items to a document in {Status} status.");

        var item = InventoryDocumentItem.Create(
            Id,
            productId,
            productVariantId,
            quantity,
            unitOfMeasure,
            inventoryId,
            inventoryLotId,
            inventorySerialId,
            description);

        Items.Add(item);

        return item;
    }

    public void RemoveItem(long itemId)
    {
        if (Status != InventoryDocumentStatus.Draft)
            throw ExceptionFactory.InvalidStatus($"Cannot remove items from a document in {Status} status.");

        var item = Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw ExceptionFactory.EntityNotFound<InventoryDocumentItem>(itemId);

        Items.Remove(item);
    }

    public void UpdateItemQuantity(long itemId, int quantity)
    {
        if (Status != InventoryDocumentStatus.Draft)
            throw ExceptionFactory.InvalidStatus($"Cannot modify items on a document in {Status} status.");

        var item = Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw ExceptionFactory.EntityNotFound<InventoryDocumentItem>(itemId);

        item.UpdateQuantity(quantity);
    }
    #endregion

    #region Workflow
    public void Submit()
    {
        if (Status != InventoryDocumentStatus.Draft)
            throw ExceptionFactory.InvalidStatus($"Cannot submit a document in {Status} status.");

        if (Items.Count == 0)
            throw ExceptionFactory.EmptyCollection("A document must contain at least one item.");

        Status = InventoryDocumentStatus.PendingApproval;
    }

    public void Approve(Guid approvedBy)
    {
        if (Status is not (InventoryDocumentStatus.Draft or InventoryDocumentStatus.PendingApproval))
            throw ExceptionFactory.InvalidStatus($"Cannot approve a document in {Status} status.");

        if (Items.Count == 0)
            throw ExceptionFactory.EmptyCollection("A document must contain at least one item.");

        Status = InventoryDocumentStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != InventoryDocumentStatus.Approved)
            throw ExceptionFactory.InvalidStatus($"Cannot complete a document in {Status} status.");

        Status = InventoryDocumentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == InventoryDocumentStatus.Completed)
            throw ExceptionFactory.InvalidStatus("Cannot cancel a completed document.");

        if (Status == InventoryDocumentStatus.Cancelled)
            throw ExceptionFactory.InvalidStatus("Document is already cancelled.");

        Status = InventoryDocumentStatus.Cancelled;
    }
    #endregion
}
