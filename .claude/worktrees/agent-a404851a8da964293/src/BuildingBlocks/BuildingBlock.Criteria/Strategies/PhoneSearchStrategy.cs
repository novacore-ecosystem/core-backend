using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

using NovaCore.BuildingBlock.Criteria.Enums;
using NovaCore.BuildingBlock.SharedKernel.Text;

namespace NovaCore.BuildingBlock.Criteria.Strategies;

/// <summary>
/// Indexed prefix/suffix phone search. Prefix (`sw`) queries the normalized <c>PhoneSearch</c> column; suffix (`ew`)
/// reverses the keyword and queries the pre-reversed <c>PhoneReverse</c> column - both compile to an indexed
/// `LIKE 'x%'`, never the un-indexable `LIKE '%x'`. Callers only ever see the logical phone field; this is the only
/// place that knows about the two extra columns or the "reverse the keyword" trick.
/// </summary>
public sealed class PhoneSearchStrategy<TEntity> : IFieldQueryStrategy<TEntity>
{
    private static readonly MethodInfo StartsWithMethod =
        typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!;

    private static readonly MethodInfo ContainsMethod =
        typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(string));

    private readonly Expression<Func<TEntity, string>> _phoneSearchAccessor;
    private readonly Expression<Func<TEntity, string>> _phoneReverseAccessor;

    public PhoneSearchStrategy(
        Expression<Func<TEntity, string>> phoneSearchAccessor,
        Expression<Func<TEntity, string>> phoneReverseAccessor)
    {
        _phoneSearchAccessor = phoneSearchAccessor;
        _phoneReverseAccessor = phoneReverseAccessor;
    }

    public Expression BuildPredicate(ParameterExpression parameter, CriteriaOperator @operator, JsonElement? value)
    {
        return @operator switch
        {
            CriteriaOperator.Eq => BuildEquals(parameter, _phoneSearchAccessor, NormalizeSingle(value)),
            CriteriaOperator.Ne => Expression.Not(BuildEquals(parameter, _phoneSearchAccessor, NormalizeSingle(value))),
            CriteriaOperator.StartsWith => BuildStartsWith(parameter, _phoneSearchAccessor, NormalizeSingle(value)),
            CriteriaOperator.EndsWith => BuildStartsWith(parameter, _phoneReverseAccessor, PhoneNormalizer.Reverse(NormalizeSingle(value))),
            CriteriaOperator.In => BuildIn(parameter, _phoneSearchAccessor, NormalizeMany(value)),
            CriteriaOperator.NotIn => Expression.Not(BuildIn(parameter, _phoneSearchAccessor, NormalizeMany(value))),
            _ => throw new NotSupportedException($"Operator '{@operator}' is not supported for phone search fields."),
        };
    }

    private static string NormalizeSingle(JsonElement? value) => PhoneNormalizer.Normalize(value?.GetString());

    private static List<string> NormalizeMany(JsonElement? value)
    {
        if (value is not { ValueKind: JsonValueKind.Array } element)
            return [];

        return element.EnumerateArray().Select(e => PhoneNormalizer.Normalize(e.GetString())).ToList();
    }

    private static Expression BuildEquals(ParameterExpression parameter, LambdaExpression accessor, string target)
    {
        var property = ParameterRebinder.ReplaceParameter(accessor, parameter);
        return Expression.Equal(property, Expression.Constant(target));
    }

    private static Expression BuildStartsWith(ParameterExpression parameter, LambdaExpression accessor, string prefix)
    {
        var property = ParameterRebinder.ReplaceParameter(accessor, parameter);
        return Expression.Call(property, StartsWithMethod, Expression.Constant(prefix));
    }

    private static Expression BuildIn(ParameterExpression parameter, LambdaExpression accessor, List<string> values)
    {
        var property = ParameterRebinder.ReplaceParameter(accessor, parameter);
        return Expression.Call(ContainsMethod, Expression.Constant(values), property);
    }
}
