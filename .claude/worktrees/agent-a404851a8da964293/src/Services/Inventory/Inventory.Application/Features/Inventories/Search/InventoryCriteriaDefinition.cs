using NovaCore.BuildingBlock.Criteria.Definition;

namespace NovaCore.Inventory.Application.Features.Inventories.Search;

/// <summary>Admin search whitelist for <see cref="InventoryStock"/>. Built once (static singleton) - no per-request reflection scan. No keyword-searchable field - stock rows have no free-text content.</summary>
public static class InventoryCriteriaDefinition
{
    public static readonly CriteriaDefinition<InventoryStock> Instance = CriteriaDefinition<InventoryStock>.Create()
        .Field(x => x.ProductId).Guid()
        .Field(x => x.VariantId).Guid()
        .Field(x => x.WarehouseId).Guid()
        .Field(x => x.AvailableQuantity).Number().Sortable()
        .Field(x => x.CreatedAt).DateTime().Sortable()
        .Build();
}
