using NovaCore.Inventory.Application.Features.Inventories.DTOs;

namespace NovaCore.Inventory.Application.Abstractions.Persistence.InventoryTransactions;


public interface IInventoryTransactionWriteService
{
    Task StageAddAsync(CreateInventoryTransactionDto dto, CancellationToken ct = default);
}
