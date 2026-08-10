using System.Linq.Expressions;
using System.Text.Json;

using NovaCore.BuildingBlock.Criteria.Enums;

namespace NovaCore.BuildingBlock.Criteria.Strategies;

/// <summary>
/// Encapsulates how a field with non-trivial query behavior (Phone, and future FullText/Json/Composite/Computed
/// fields) turns an operator + raw value into a predicate. Implementations build the boolean <see cref="Expression"/>
/// against the shared <paramref name="parameter"/> passed in, so it composes with every other filter's predicate in
/// one final lambda. The Criteria Builder only ever calls <see cref="BuildPredicate"/> - it never special-cases a
/// field by name.
/// </summary>
public interface IFieldQueryStrategy<TEntity>
{
    Expression BuildPredicate(ParameterExpression parameter, CriteriaOperator @operator, JsonElement? value);
}
