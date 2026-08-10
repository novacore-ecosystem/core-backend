using NovaCore.BuildingBlock.Criteria.Definition;

namespace NovaCore.Inventory.Application.Features.Warehouses.Search;

/// <summary>Admin search whitelist for <see cref="Warehouse"/>. Built once (static singleton) - no per-request reflection scan.</summary>
public static class WarehouseCriteriaDefinition
{
    public static readonly CriteriaDefinition<Warehouse> Instance = CriteriaDefinition<Warehouse>.Create()
        .Field(x => x.Code).String().Sortable().KeywordSearchable()
        .Field(x => x.Name).String().Sortable().KeywordSearchable()
        .Field(x => x.Address).String().KeywordSearchable()
        .Field(x => x.Status).Enum().Sortable()
        .Field(x => x.CreatedAt).DateTime().Sortable()
        .Build();
}
