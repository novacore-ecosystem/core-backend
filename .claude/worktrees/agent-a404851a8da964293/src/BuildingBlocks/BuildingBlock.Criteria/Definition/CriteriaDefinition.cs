using System.Linq.Expressions;

namespace NovaCore.BuildingBlock.Criteria.Definition;

/// <summary>
/// The Fluent field whitelist for one entity's Admin search. A field is reachable by a <c>CriteriaRequest</c> filter
/// or sort only if it was explicitly declared here via <see cref="Field{TProp}"/> - there is no reflection scan of
/// the entity, and this definition is built once (a static singleton per entity), not per request.
/// </summary>
public sealed class CriteriaDefinition<TEntity>
{
    private readonly Dictionary<string, CriteriaFieldMetadata<TEntity>> _fields = new(StringComparer.OrdinalIgnoreCase);

    private CriteriaDefinition()
    {
    }

    public static CriteriaDefinition<TEntity> Create() => new();

    /// <summary>
    /// Declares a searchable field. <paramref name="name"/> is the logical name exposed to API consumers - it
    /// defaults to the accessor's member name, but pass it explicitly whenever the public field name should differ
    /// from the property name (e.g. logical `phone` over an entity's `PhoneNumber` property), so the request
    /// contract never leaks the underlying member/column name.
    /// </summary>
    public CriteriaFieldBuilder<TEntity, TProp> Field<TProp>(Expression<Func<TEntity, TProp>> accessor, string? name = null)
    {
        var logicalName = name ?? ExtractMemberName(accessor);
        return new CriteriaFieldBuilder<TEntity, TProp>(this, logicalName, accessor);
    }

    public CriteriaDefinition<TEntity> Build() => this;

    public bool TryGetField(string name, out CriteriaFieldMetadata<TEntity> field)
        => _fields.TryGetValue(name, out field!);

    public IReadOnlyCollection<CriteriaFieldMetadata<TEntity>> KeywordFields
        => [.. _fields.Values.Where(f => f.KeywordSearchable)];

    internal void Register(CriteriaFieldMetadata<TEntity> metadata) => _fields[metadata.LogicalName] = metadata;

    private static string ExtractMemberName<TProp>(Expression<Func<TEntity, TProp>> accessor)
    {
        var body = accessor.Body is UnaryExpression { Operand: MemberExpression inner } ? inner : accessor.Body;

        if (body is MemberExpression member)
            return member.Member.Name;

        throw new ArgumentException("Field accessor must be a simple property access expression, e.g. x => x.Prop.", nameof(accessor));
    }
}
