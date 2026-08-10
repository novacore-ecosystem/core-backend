using NovaCore.BuildingBlock.Criteria.Enums;

namespace NovaCore.BuildingBlock.Criteria.Definition;

/// <summary>Per-type default operator whitelist, per the framework's operator-validation table. `.AllowOperators(...)` narrows or overrides these on a specific field.</summary>
internal static class DefaultOperators
{
    public static readonly IReadOnlySet<CriteriaOperator> None = new HashSet<CriteriaOperator>();

    public static IReadOnlySet<CriteriaOperator> For(CriteriaFieldType type) => type switch
    {
        CriteriaFieldType.String => new HashSet<CriteriaOperator>
        {
            CriteriaOperator.Eq, CriteriaOperator.Ne, CriteriaOperator.Contains,
            CriteriaOperator.StartsWith, CriteriaOperator.EndsWith, CriteriaOperator.In, CriteriaOperator.NotIn,
        },
        CriteriaFieldType.Number => new HashSet<CriteriaOperator>
        {
            CriteriaOperator.Eq, CriteriaOperator.Ne, CriteriaOperator.Gt, CriteriaOperator.Gte,
            CriteriaOperator.Lt, CriteriaOperator.Lte, CriteriaOperator.In, CriteriaOperator.NotIn,
        },
        CriteriaFieldType.DateTime => new HashSet<CriteriaOperator>
        {
            CriteriaOperator.Eq, CriteriaOperator.Ne, CriteriaOperator.Gt, CriteriaOperator.Gte,
            CriteriaOperator.Lt, CriteriaOperator.Lte, CriteriaOperator.Between,
        },
        CriteriaFieldType.Boolean => new HashSet<CriteriaOperator> { CriteriaOperator.Eq, CriteriaOperator.Ne },
        CriteriaFieldType.Guid => new HashSet<CriteriaOperator>
        {
            CriteriaOperator.Eq, CriteriaOperator.Ne, CriteriaOperator.In, CriteriaOperator.NotIn,
        },
        CriteriaFieldType.Enum => new HashSet<CriteriaOperator>
        {
            CriteriaOperator.Eq, CriteriaOperator.Ne, CriteriaOperator.In, CriteriaOperator.NotIn,
        },
        CriteriaFieldType.ValueObject => new HashSet<CriteriaOperator> { CriteriaOperator.Eq, CriteriaOperator.Ne },
        _ => None,
    };
}
