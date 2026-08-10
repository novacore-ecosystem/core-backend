using NovaCore.BuildingBlock.Criteria.Definition;
using NovaCore.BuildingBlock.Criteria.Strategies;

namespace NovaCore.Order.Application.Features.Orders.Search;

/// <summary>Admin search whitelist for <see cref="OrderEntity"/>. Built once (static singleton) - no per-request reflection scan.</summary>
public static class OrderCriteriaDefinition
{
    public static readonly CriteriaDefinition<OrderEntity> Instance = CriteriaDefinition<OrderEntity>.Create()
        .Field(x => x.Id).Guid()
        .Field(x => x.Owner.OwnerName).String().Sortable().KeywordSearchable().IgnoreCase()
        .Field(x => x.Owner.OwnerPhone.Value, name: "phone").UsePhoneSearch(x => x.Owner.OwnerPhoneSearch, x => x.Owner.OwnerPhoneReverse)
        .Field(x => x.Status).Enum().Sortable()
        .Field(x => x.CreatedAt).DateTime().Sortable()
        .Build();
}
