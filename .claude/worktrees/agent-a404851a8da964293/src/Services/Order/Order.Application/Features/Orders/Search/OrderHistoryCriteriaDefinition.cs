using NovaCore.BuildingBlock.Criteria.Definition;

namespace NovaCore.Order.Application.Features.Orders.Search;

/// <summary>
/// Customer-facing order-history whitelist for <see cref="OrderEntity"/> - deliberately narrower
/// than <see cref="OrderCriteriaDefinition"/> (no CustomerName/Phone: the caller can only ever see
/// their own orders, so those fields are constant for every row and pointless to filter/sort by).
/// </summary>
public static class OrderHistoryCriteriaDefinition
{
    public static readonly CriteriaDefinition<OrderEntity> Instance = CriteriaDefinition<OrderEntity>.Create()
        .Field(x => x.Status).Enum().Sortable()
        .Field(x => x.CreatedAt).DateTime().Sortable()
        .Build();
}
