using System.Linq.Expressions;

using NovaCore.BuildingBlock.Criteria.Enums;
using NovaCore.BuildingBlock.Criteria.Strategies;

namespace NovaCore.BuildingBlock.Criteria.Definition;

/// <summary>The type-declaration half of the Fluent DSL: `.Field(x => x.Prop)` returns this, then exactly one of `.String()/.Number()/.../UseStrategy(...)` registers the field with its type-appropriate default operator whitelist.</summary>
public sealed class CriteriaFieldBuilder<TEntity, TProp>
{
    private readonly CriteriaDefinition<TEntity> _definition;
    private readonly string _fieldName;
    private readonly LambdaExpression _accessor;

    internal CriteriaFieldBuilder(CriteriaDefinition<TEntity> definition, string fieldName, Expression<Func<TEntity, TProp>> accessor)
    {
        _definition = definition;
        _fieldName = fieldName;
        _accessor = accessor;
    }

    public ConfiguredField<TEntity> String() => Register(CriteriaFieldType.String);
    public ConfiguredField<TEntity> Number() => Register(CriteriaFieldType.Number);
    public ConfiguredField<TEntity> DateTime() => Register(CriteriaFieldType.DateTime);
    public ConfiguredField<TEntity> Boolean() => Register(CriteriaFieldType.Boolean);
    public ConfiguredField<TEntity> Guid() => Register(CriteriaFieldType.Guid);
    public ConfiguredField<TEntity> Enum() => Register(CriteriaFieldType.Enum);
    public ConfiguredField<TEntity> ValueObject() => Register(CriteriaFieldType.ValueObject);

    public ConfiguredField<TEntity> UseStrategy(IFieldQueryStrategy<TEntity> strategy, params CriteriaOperator[] allowedOperators)
    {
        _definition.Register(new CriteriaFieldMetadata<TEntity>
        {
            LogicalName = _fieldName,
            Accessor = _accessor,
            FieldType = CriteriaFieldType.Custom,
            AllowedOperators = allowedOperators.Length > 0 ? allowedOperators.ToHashSet() : DefaultOperators.None,
            Strategy = strategy,
        });
        return new ConfiguredField<TEntity>(_definition, _fieldName);
    }

    private ConfiguredField<TEntity> Register(CriteriaFieldType fieldType)
    {
        _definition.Register(new CriteriaFieldMetadata<TEntity>
        {
            LogicalName = _fieldName,
            Accessor = _accessor,
            FieldType = fieldType,
            AllowedOperators = DefaultOperators.For(fieldType),
        });
        return new ConfiguredField<TEntity>(_definition, _fieldName);
    }
}
