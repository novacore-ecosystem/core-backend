using NovaCore.BuildingBlock.Criteria.Definition;

namespace NovaCore.Inventory.Application.Features.InventoryDocuments.Search;

public static class InventoryDocumentCriteriaDefinition
{
    public static readonly CriteriaDefinition<InventoryDocument> Instance = CriteriaDefinition<InventoryDocument>.Create()
        .Field(x => x.Number).String().Sortable()
        .Field(x => x.Type).Enum().Sortable()
        .Field(x => x.Reason).Enum().Sortable()
        .Field(x => x.Status).Enum().Sortable()
        .Field(x => x.SourceWarehouseId).Guid()
        .Field(x => x.DestinationWarehouseId).Guid()
        .Field(x => x.CreatedAt).DateTime().Sortable()
        .Build();
}
