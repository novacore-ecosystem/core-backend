using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryDocuments;
using NovaCore.Inventory.Persistence.Contexts.InventoryDocuments.Repositories;

namespace NovaCore.Inventory.Persistence.Contexts.InventoryDocuments.Write;

/// <summary>
/// Never calls IUnitOfWork itself - callers (InventoryDocumentService) build and mutate the
/// entity (status transitions, AddItem) in-memory before and after this call, and the owning
/// handler's own ExecuteTransactionAsync performs the single SaveChanges for the whole batch.
/// </summary>
public sealed class InventoryDocumentWriteService(
    IInventoryDocumentRepository repo) : IInventoryDocumentWriteService
{
    public async Task AddAsync(InventoryDocument entity, CancellationToken ct = default)
    {
        await repo.AddAsync(entity, ct);
    }
}
