using NovaCore.BuildingBlock.Criteria.Definition;

namespace NovaCore.Inventory.Application.Features.InventoryCounts.Search;

public static class InventoryCountCriteriaDefinition
{
    public static readonly CriteriaDefinition<InventoryCount> Instance = CriteriaDefinition<InventoryCount>.Create()
        .Field(x => x.Number).String().Sortable()
        .Field(x => x.WarehouseId).Guid()
        .Field(x => x.Status).Enum().Sortable()
        .Field(x => x.CountDate).DateTime().Sortable()
        .Field(x => x.CreatedAt).DateTime().Sortable()
        .Build();
}
