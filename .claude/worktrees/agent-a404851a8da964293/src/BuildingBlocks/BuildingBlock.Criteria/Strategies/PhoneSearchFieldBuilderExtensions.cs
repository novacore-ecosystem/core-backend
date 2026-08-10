using System.Linq.Expressions;

using NovaCore.BuildingBlock.Criteria.Definition;
using NovaCore.BuildingBlock.Criteria.Enums;

namespace NovaCore.BuildingBlock.Criteria.Strategies;

public static class PhoneSearchFieldBuilderExtensions
{
    private static readonly CriteriaOperator[] AllowedOperators =
    [
        CriteriaOperator.Eq,
        CriteriaOperator.Ne,
        CriteriaOperator.StartsWith,
        CriteriaOperator.EndsWith,
        CriteriaOperator.In,
        CriteriaOperator.NotIn,
    ];

    /// <summary>
    /// Wires up indexed prefix/suffix phone search. Deliberately excludes `Contains` from the allowed operators -
    /// the framework structurally forbids the `LIKE '%x%'` full-scan for phone fields, not just by convention.
    /// </summary>
    public static ConfiguredField<TEntity> UsePhoneSearch<TEntity>(
        this CriteriaFieldBuilder<TEntity, string> builder,
        Expression<Func<TEntity, string>> phoneSearchAccessor,
        Expression<Func<TEntity, string>> phoneReverseAccessor)
    {
        var strategy = new PhoneSearchStrategy<TEntity>(phoneSearchAccessor, phoneReverseAccessor);
        return builder.UseStrategy(strategy, AllowedOperators);
    }
}
