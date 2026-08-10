using System.Linq.Expressions;

namespace NovaCore.BuildingBlock.Persistence.Audit;

public sealed class AuditHierarchyBuilder : IAuditHierarchyBuilder
{
    private readonly Dictionary<Type, AuditEntityMetadata> _metadataByType = [];

    public IAuditEntityHierarchyBuilder<TEntity> Entity<TEntity>() where TEntity : class
        => new AuditEntityHierarchyBuilder<TEntity>(this);

    internal void Register(Type entityType, AuditEntityMetadata metadata) => _metadataByType[entityType] = metadata;

    internal IReadOnlyDictionary<Type, AuditEntityMetadata> Build() => _metadataByType;
}

internal sealed class AuditEntityHierarchyBuilder<TEntity>(AuditHierarchyBuilder owner) : IAuditEntityHierarchyBuilder<TEntity>
    where TEntity : class
{
    public IAuditEntityHierarchyBuilder<TEntity> IsRoot<TKey>(Expression<Func<TEntity, TKey>> idSelector)
    {
        owner.Register(typeof(TEntity), new AuditEntityMetadata
        {
            EntityType = typeof(TEntity),
            IsRoot = true,
            IdAccessor = Compile(idSelector),
        });

        return this;
    }

    public IAuditEntityHierarchyBuilder<TEntity> BelongsTo<TParent>(Expression<Func<TEntity, object>> parentIdSelector)
        where TParent : class
    {
        owner.Register(typeof(TEntity), new AuditEntityMetadata
        {
            EntityType = typeof(TEntity),
            IsRoot = false,
            ParentType = typeof(TParent),
            ParentIdAccessor = Compile(parentIdSelector),
        });

        return this;
    }

    private static Func<object, object?> Compile<TKey>(Expression<Func<TEntity, TKey>> selector)
    {
        var parameter = Expression.Parameter(typeof(object), "entity");
        var invoke = Expression.Invoke(selector, Expression.Convert(parameter, typeof(TEntity)));
        var boxed = Expression.Convert(invoke, typeof(object));

        return Expression.Lambda<Func<object, object?>>(boxed, parameter).Compile();
    }
}
