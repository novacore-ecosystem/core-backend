namespace NovaCore.Inventory.Domain.Entities.InventoryDocuments.Data;

/// <summary>
/// Everything InventoryDocument.Create needs to build itself plus its Items in one call - mirrors
/// Order.Create(CreateOrderData) so InventoryDocument owns constructing its own line items instead
/// of receiving them pre-built.
/// </summary>
public sealed record CreateInventoryDocumentData(
    string Number,
    InventoryDocumentType Type,
    InventoryDocumentReason Reason,
    Guid? SourceWarehouseId,
    Guid? DestinationWarehouseId,
    string Description,
    IReadOnlyList<CreateInventoryDocumentItemData> Items);

public sealed record CreateInventoryDocumentItemData(
    Guid ProductId,
    Guid VariantId,
    int Quantity,
    string UnitOfMeasure,
    Guid? InventoryId = null,
    Guid? InventoryLotId = null,
    Guid? InventorySerialId = null,
    string Description = "");
