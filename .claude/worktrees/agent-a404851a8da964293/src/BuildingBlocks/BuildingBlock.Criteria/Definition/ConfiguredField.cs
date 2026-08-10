using System.Linq.Expressions;

using NovaCore.BuildingBlock.Criteria.Enums;

namespace NovaCore.BuildingBlock.Criteria.Definition;

/// <summary>Modifiers for the field most recently configured, plus a passthrough back to `.Field(...)` so the Fluent chain can move on to the next field.</summary>
public sealed class ConfiguredField<TEntity>
{
    private readonly CriteriaDefinition<TEntity> _definition;
    private readonly string _fieldName;

    internal ConfiguredField(CriteriaDefinition<TEntity> definition, string fieldName)
    {
        _definition = definition;
        _fieldName = fieldName;
    }

    public ConfiguredField<TEntity> Sortable()
    {
        Reconfigure(field => field with { Sortable = true });
        return this;
    }

    public ConfiguredField<TEntity> KeywordSearchable()
    {
        Reconfigure(field => field with { KeywordSearchable = true });
        return this;
    }

    /// <summary>Matches this field's Contains/StartsWith/EndsWith/Eq/Ne operators and keyword search case-insensitively (via SQL-translatable `.ToLower()`, not culture-invariant casing).</summary>
    public ConfiguredField<TEntity> IgnoreCase()
    {
        Reconfigure(field => field with { IgnoreCase = true });
        return this;
    }

    public ConfiguredField<TEntity> AllowOperators(params CriteriaOperator[] operators)
    {
        Reconfigure(field => field with { AllowedOperators = operators.ToHashSet() });
        return this;
    }

    public CriteriaFieldBuilder<TEntity, TProp> Field<TProp>(Expression<Func<TEntity, TProp>> accessor, string? name = null)
        => _definition.Field(accessor, name);

    public CriteriaDefinition<TEntity> Build() => _definition.Build();

    private void Reconfigure(Func<CriteriaFieldMetadata<TEntity>, CriteriaFieldMetadata<TEntity>> mutate)
    {
        _definition.TryGetField(_fieldName, out var current);
        _definition.Register(mutate(current));
    }
}
