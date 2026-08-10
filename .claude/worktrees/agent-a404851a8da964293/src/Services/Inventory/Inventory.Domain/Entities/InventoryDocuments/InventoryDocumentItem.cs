using NovaCore.Inventory.Domain.Entities.Inventories;
using NovaCore.Inventory.Domain.Metadata;

namespace NovaCore.Inventory.Domain.Entities.InventoryDocuments;

public sealed class InventoryDocumentItem : BaseEntity<long>, IAuditable, ITenantEntity
{
    public Guid InventoryDocumentId { get; private set; }
    public InventoryDocument Document { get; private set; } = default!;
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public Guid? InventoryId { get; private set; }
    public InventoryStock? Inventory { get; private set; }
    public Quantity Quantity { get; private set; } = default!;
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public Guid? InventoryLotId { get; private set; }
    public Guid? InventorySerialId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public InventoryDocumentItemMetadata Metadata { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private InventoryDocumentItem() { }

    /// <summary>Only InventoryDocument may construct/mutate its Items - see InventoryDocument.AddItem.</summary>
    internal static InventoryDocumentItem Create(
        Guid inventoryDocumentId,
        Guid productId,
        Guid productVariantId,
        int quantity,
        string unitOfMeasure,
        Guid? inventoryId,
        Guid? inventoryLotId,
        Guid? inventorySerialId,
        string description)
    {
        if (unitOfMeasure.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Unit of measure is required.");

        return new InventoryDocumentItem
        {
            InventoryDocumentId = inventoryDocumentId,
            ProductId = productId,
            VariantId = productVariantId,
            Quantity = Quantity.Create(quantity),
            UnitOfMeasure = unitOfMeasure.Trim(),
            InventoryId = inventoryId,
            InventoryLotId = inventoryLotId,
            InventorySerialId = inventorySerialId,
            Description = description,
            Metadata = InventoryDocumentItemMetadata.Create<InventoryDocumentItemMetadata>(),
        };
    }

    /// <summary>Only InventoryDocument may mutate its Items - see InventoryDocument.UpdateItemQuantity.</summary>
    internal void UpdateQuantity(int quantity)
    {
        Quantity = Quantity.Create(quantity);
    }
}
