using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

using NovaCore.BuildingBlock.Criteria.Enums;

namespace NovaCore.BuildingBlock.Criteria.Strategies;

/// <summary>
/// Filters a string-collection field (e.g. a Postgres <c>text[]</c> column like Roles) by containment
/// rather than equality - Eq/Ne check whether the collection contains one value. This is the reverse of
/// the string-field Contains operator (substring match on a single string property).
/// </summary>
public sealed class StringCollectionContainsStrategy<TEntity> : IFieldQueryStrategy<TEntity>
{
    private static readonly MethodInfo ContainsMethod =
        typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(string));

    private readonly Expression<Func<TEntity, IEnumerable<string>>> _accessor;

    public StringCollectionContainsStrategy(Expression<Func<TEntity, IEnumerable<string>>> accessor)
    {
        _accessor = accessor;
    }

    public Expression BuildPredicate(ParameterExpression parameter, CriteriaOperator @operator, JsonElement? value)
    {
        var property = ParameterRebinder.ReplaceParameter(_accessor, parameter);
        var contains = BuildContains(property, value?.GetString());

        return @operator switch
        {
            CriteriaOperator.Eq => contains,
            CriteriaOperator.Ne => Expression.Not(contains),
            _ => throw new NotSupportedException($"Operator '{@operator}' is not supported for collection-contains fields."),
        };
    }

    private static Expression BuildContains(Expression property, string? item)
        => Expression.Call(ContainsMethod, property, Expression.Constant(item, typeof(string)));
}
