using NovaCore.BuildingBlock.Criteria.Definition;

namespace NovaCore.Inventory.Application.Features.Inventories.Search;

/// <summary>Admin search whitelist for <see cref="InventoryTransaction"/> (stock movements). Built once (static singleton) - no per-request reflection scan.</summary>
public static class InventoryTransactionCriteriaDefinition
{
    public static readonly CriteriaDefinition<InventoryTransaction> Instance = CriteriaDefinition<InventoryTransaction>.Create()
        .Field(x => x.InventoryId).Guid()
        .Field(x => x.ProductId).Guid()
        .Field(x => x.VariantId).Guid()
        .Field(x => x.WarehouseId).Guid()
        .Field(x => x.Type).Enum().Sortable()
        .Field(x => x.BeforeOnHandQuantity).Number().Sortable()
        .Field(x => x.AfterOnHandQuantity).Number().Sortable()
        .Field(x => x.Description).String().KeywordSearchable()
        .Field(x => x.CreatedAt).DateTime().Sortable()
        .Build();
}
