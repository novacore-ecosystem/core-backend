using System.Linq.Expressions;
using System.Reflection;

using NovaCore.BuildingBlock.Domain.Abstractions;

namespace NovaCore.BuildingBlock.Persistence.Ef.Repository.Helpers;

public static class PredicateExpressionBuilder
{
    #region Reflection Cache

    private static readonly MethodInfo EnumerableContainsMethod = typeof(Enumerable)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Single(method =>
            method.Name == nameof(Enumerable.Contains)
            && method.GetParameters().Length == 2);

    #endregion

    #region Core Builders

    private static Expression<Func<TEntity, bool>> BuildBinaryExpression<TEntity, TValue>(
        Expression<Func<TEntity, TValue>> selector,
        TValue value,
        Func<Expression, Expression, BinaryExpression> comparison)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(comparison);

        var constant = CreateConstant(selector.Body.Type, value);

        return Expression.Lambda<Func<TEntity, bool>>(
            comparison(selector.Body, constant),
            selector.Parameters);
    }

    private static Expression<Func<TEntity, bool>> BuildContainsExpression<TEntity, TValue>(
        Expression<Func<TEntity, TValue>> selector,
        IEnumerable<TValue> values,
        bool negate)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(values);

        var collection = Materialize(values);

        var containsMethod = EnumerableContainsMethod.MakeGenericMethod(typeof(TValue));

        var containsCall = Expression.Call(
            containsMethod,
            Expression.Constant(collection),
            selector.Body);

        Expression body = negate
            ? Expression.Not(containsCall)
            : containsCall;

        return Expression.Lambda<Func<TEntity, bool>>(
            body,
            selector.Parameters);
    }

    private static TValue[] Materialize<TValue>(
        IEnumerable<TValue> values)
    {
        return values switch
        {
            TValue[] array => array,
            List<TValue> list => [.. list],
            HashSet<TValue> hashSet => [.. hashSet],
            IReadOnlyCollection<TValue> readOnly => [.. readOnly],
            ICollection<TValue> collection => [.. collection],
            _ => [.. values]
        };
    }

    private static ConstantExpression CreateConstant<TValue>(
        Type targetType,
        TValue value)
    {
        return Expression.Constant(value, targetType);
    }

    private static Expression ReplaceParameter(
        Expression expression,
        ParameterExpression source,
        ParameterExpression target)
    {
        return new ParameterReplaceVisitor(
            source,
            target)
            .Visit(expression)!;
    }

    private static Expression<Func<TEntity, bool>> Combine<TEntity>(
        Expression<Func<TEntity, bool>> left,
        Expression<Func<TEntity, bool>> right,
        Func<Expression, Expression, BinaryExpression> operation)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        ArgumentNullException.ThrowIfNull(operation);

        var parameter = Expression.Parameter(typeof(TEntity), "entity");

        // EF Core requires both predicates to share the same parameter instance
        // before they can be translated into a single SQL expression.
        var leftBody = ReplaceParameter(
            left.Body,
            left.Parameters[0],
            parameter);

        var rightBody = ReplaceParameter(
            right.Body,
            right.Parameters[0],
            parameter);

        return Expression.Lambda<Func<TEntity, bool>>(
            operation(leftBody, rightBody),
            parameter);
    }

    #endregion

    #region Expression Visitor

    /// <summary>
    /// Replaces a parameter instance inside an expression tree.
    /// </summary>
    private sealed class ParameterReplaceVisitor(
        ParameterExpression source,
        ParameterExpression target)
        : ExpressionVisitor
    {
        private readonly ParameterExpression _source = source;
        private readonly ParameterExpression _target = target;

        protected override Expression VisitParameter(
            ParameterExpression node)
        {
            return node == _source
                ? _target
                : base.VisitParameter(node);
        }
    }

    #endregion

    #region Predicate Combinators

    /// <summary>
    /// Combines two predicates using logical AND.
    /// </summary>
    public static Expression<Func<TEntity, bool>> And<TEntity>(
        Expression<Func<TEntity, bool>> left,
        Expression<Func<TEntity, bool>> right)
    {
        return Combine(
            left,
            right,
            Expression.AndAlso);
    }

    /// <summary>
    /// Combines two predicates using logical OR.
    /// </summary>
    public static Expression<Func<TEntity, bool>> Or<TEntity>(
        Expression<Func<TEntity, bool>> left,
        Expression<Func<TEntity, bool>> right)
    {
        return Combine(
            left,
            right,
            Expression.OrElse);
    }

    /// <summary>
    /// Negates a predicate.
    /// </summary>
    public static Expression<Func<TEntity, bool>> Not<TEntity>(
        Expression<Func<TEntity, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return Expression.Lambda<Func<TEntity, bool>>(
            Expression.Not(predicate.Body),
            predicate.Parameters);
    }

    #endregion

    #region Comparison & Membership Builders

    /// <summary>
    /// Builds an equality predicate: <c>entity => selector(entity) == value</c>.
    /// </summary>
    public static Expression<Func<TEntity, bool>> BuildEquals<TEntity, TValue>(
        Expression<Func<TEntity, TValue>> selector,
        TValue value)
    {
        return BuildBinaryExpression(selector, value, Expression.Equal);
    }

    /// <summary>
    /// Builds an inequality predicate: <c>entity => selector(entity) != value</c>.
    /// </summary>
    public static Expression<Func<TEntity, bool>> BuildNotEquals<TEntity, TValue>(
        Expression<Func<TEntity, TValue>> selector,
        TValue value)
    {
        return BuildBinaryExpression(selector, value, Expression.NotEqual);
    }

    /// <summary>
    /// Builds a membership predicate: <c>entity => values.Contains(selector(entity))</c>.
    /// </summary>
    public static Expression<Func<TEntity, bool>> BuildIn<TEntity, TValue>(
        Expression<Func<TEntity, TValue>> selector,
        IEnumerable<TValue> values)
    {
        return BuildContainsExpression(selector, values, negate: false);
    }

    /// <summary>
    /// Builds a membership predicate: <c>entity => values.Contains(selector(entity))</c>.
    /// </summary>
    public static Expression<Func<TEntity, bool>> BuildIn<TEntity, TValue>(
        Expression<Func<TEntity, TValue>> selector,
        params TValue[] values)
    {
        return BuildContainsExpression(selector, values, negate: false);
    }

    /// <summary>
    /// Builds a negated membership predicate: <c>entity => !values.Contains(selector(entity))</c>.
    /// </summary>
    public static Expression<Func<TEntity, bool>> BuildNotIn<TEntity, TValue>(
        Expression<Func<TEntity, TValue>> selector,
        IEnumerable<TValue> values)
    {
        return BuildContainsExpression(selector, values, negate: true);
    }

    /// <summary>
    /// Builds a negated membership predicate: <c>entity => !values.Contains(selector(entity))</c>.
    /// </summary>
    public static Expression<Func<TEntity, bool>> BuildNotIn<TEntity, TValue>(
        Expression<Func<TEntity, TValue>> selector,
        params TValue[] values)
    {
        return BuildContainsExpression(selector, values, negate: true);
    }

    /// <summary>
    /// Builds an Id equality predicate: <c>entity => entity.Id == id</c>.
    /// </summary>
    public static Expression<Func<TEntity, bool>> BuildIdEquals<TEntity, TId>(
        TId id)
        where TEntity : IEntity<TId>
    {
        return BuildBinaryExpression<TEntity, TId>(e => e.Id, id, Expression.Equal);
    }

    /// <summary>
    /// Builds an Id inequality predicate: <c>entity => entity.Id != id</c>.
    /// </summary>
    public static Expression<Func<TEntity, bool>> BuildIdNotEquals<TEntity, TId>(
        TId id)
        where TEntity : IEntity<TId>
    {
        return BuildBinaryExpression<TEntity, TId>(e => e.Id, id, Expression.NotEqual);
    }

    /// <summary>
    /// Builds an Id membership predicate: <c>entity => ids.Contains(entity.Id)</c>.
    /// </summary>
    public static Expression<Func<TEntity, bool>> BuildIdIn<TEntity, TId>(
        IEnumerable<TId> ids)
        where TEntity : IEntity<TId>
    {
        return BuildContainsExpression<TEntity, TId>(e => e.Id, ids, negate: false);
    }

    /// <summary>
    /// Builds an Id membership predicate: <c>entity => ids.Contains(entity.Id)</c>.
    /// </summary>
    public static Expression<Func<TEntity, bool>> BuildIdIn<TEntity, TId>(
        params TId[] ids)
        where TEntity : IEntity<TId>
    {
        return BuildContainsExpression<TEntity, TId>(e => e.Id, ids, negate: false);
    }

    /// <summary>
    /// Builds a negated Id membership predicate: <c>entity => !ids.Contains(entity.Id)</c>.
    /// </summary>
    public static Expression<Func<TEntity, bool>> BuildIdNotIn<TEntity, TId>(
        IEnumerable<TId> ids)
        where TEntity : IEntity<TId>
    {
        return BuildContainsExpression<TEntity, TId>(e => e.Id, ids, negate: true);
    }

    /// <summary>
    /// Builds a negated Id membership predicate: <c>entity => !ids.Contains(entity.Id)</c>.
    /// </summary>
    public static Expression<Func<TEntity, bool>> BuildIdNotIn<TEntity, TId>(
        params TId[] ids)
        where TEntity : IEntity<TId>
    {
        return BuildContainsExpression<TEntity, TId>(e => e.Id, ids, negate: true);
    }

    #endregion
}
