using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Inventory.Application.Abstractions.Persistence.Inventories;

using Mapster;

namespace NovaCore.Inventory.Application.Features.Inventories.Queries.GetInventory;

public sealed class GetInventoryHandler(IInventoryReadService inventoryReadService)
    : IQueryHandler<GetInventoryQuery, GetInventoryResponse>
{
    public async Task<GetInventoryResponse> Handle(GetInventoryQuery request, CancellationToken ct = default)
    {
        var inventory = await inventoryReadService.GetByIdAsync(request.InventoryId, ct)
            ?? throw new NotFoundException("Inventory", request.InventoryId);

        return inventory.Adapt<GetInventoryResponse>();
    }
}
