using NovaCore.BuildingBlock.Criteria.Definition;

namespace NovaCore.Inventory.Application.Features.InventoryReservations.Search;

public static class InventoryReservationCriteriaDefinition
{
    public static readonly CriteriaDefinition<InventoryReservation> Instance = CriteriaDefinition<InventoryReservation>.Create()
        .Field(x => x.Number).String().Sortable()
        .Field(x => x.InventoryId).Guid()
        .Field(x => x.WarehouseId).Guid()
        .Field(x => x.VariantId).Guid()
        .Field(x => x.Type).Enum().Sortable()
        .Field(x => x.Status).Enum().Sortable()
        .Field(x => x.CreatedAt).DateTime().Sortable()
        .Build();
}
