using NovaCore.BuildingBlock.Criteria.Definition;

namespace NovaCore.Inventory.Application.Features.InventorySerials.Search;

public static class InventorySerialCriteriaDefinition
{
    public static readonly CriteriaDefinition<InventorySerial> Instance = CriteriaDefinition<InventorySerial>.Create()
        .Field(x => x.InventoryId).Guid()
        .Field(x => x.SerialNumber).String().Sortable()
        .Field(x => x.Status).Enum().Sortable()
        .Field(x => x.CreatedAt).DateTime().Sortable()
        .Build();
}
