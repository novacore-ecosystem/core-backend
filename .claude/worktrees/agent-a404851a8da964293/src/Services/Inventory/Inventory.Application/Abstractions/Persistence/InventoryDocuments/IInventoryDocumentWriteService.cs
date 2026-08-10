namespace NovaCore.Inventory.Application.Abstractions.Persistence.InventoryDocuments;

public interface IInventoryDocumentWriteService
{
    Task AddAsync(InventoryDocument entity, CancellationToken ct = default);
}
