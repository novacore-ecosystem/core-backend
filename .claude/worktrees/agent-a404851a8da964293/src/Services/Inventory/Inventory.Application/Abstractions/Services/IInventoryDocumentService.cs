using NovaCore.BuildingBlock.Application.Abstractions.Services;

namespace NovaCore.Inventory.Application.Abstractions.Services;

public interface IInventoryDocumentService : IService
{
    Task<InventoryDocument> CreateAndCompleteAsync(
        InventoryDocumentType type,
        InventoryDocumentReason reason,
        Guid? sourceWarehouseId,
        Guid? destinationWarehouseId,
        string description,
        CancellationToken ct = default);

    Task<InventoryDocument> CreateAndCompleteAsync(
        string number,
        InventoryDocumentType type,
        InventoryDocumentReason reason,
        Guid? sourceWarehouseId,
        Guid? destinationWarehouseId,
        string description,
        CancellationToken ct = default);

    InventoryDocument CreateDraft(
        InventoryDocumentType type,
        InventoryDocumentReason reason,
        Guid? sourceWarehouseId,
        Guid? destinationWarehouseId,
        string description = "");

    void CompleteWorkflow(InventoryDocument document);

    Task SaveAsync(InventoryDocument document, CancellationToken ct = default);

    Task<InventoryDocument?> GetByNumberAsync(string number, CancellationToken ct = default);
}
