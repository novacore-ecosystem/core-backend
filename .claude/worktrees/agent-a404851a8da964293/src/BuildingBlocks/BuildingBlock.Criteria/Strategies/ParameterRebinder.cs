using System.Linq.Expressions;

namespace NovaCore.BuildingBlock.Criteria.Strategies;

/// <summary>Splices a standalone field-accessor lambda (`x => x.Prop`) onto a different, shared parameter, so several fields' accessors can be combined into one expression tree.</summary>
internal sealed class ParameterRebinder : ExpressionVisitor
{
    private readonly ParameterExpression _from;
    private readonly ParameterExpression _to;

    private ParameterRebinder(ParameterExpression from, ParameterExpression to)
    {
        _from = from;
        _to = to;
    }

    public static Expression ReplaceParameter(LambdaExpression lambda, ParameterExpression parameter)
    {
        var rebinder = new ParameterRebinder(lambda.Parameters[0], parameter);
        return rebinder.Visit(lambda.Body);
    }

    protected override Expression VisitParameter(ParameterExpression node) => node == _from ? _to : node;
}
