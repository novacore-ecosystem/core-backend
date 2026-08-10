using System.Linq.Expressions;

namespace NovaCore.BuildingBlock.Persistence.Audit;

/// <summary>
/// Strongly-typed, expression-based, compile-time-safe registration surface for the audit
/// hierarchy. Developers configure only the direct parent of each type (<c>BelongsTo{TParent}</c>)
/// - the framework resolves the full ancestor chain up to the root automatically, at both
/// startup (validation, see <see cref="AuditHierarchyRegistry"/>) and per-instance graph
/// building (see <c>AuditGraphBuilder</c>). Relationships are never guessed from reflection,
/// EF navigations, or naming conventions - only what's registered here is ever used.
/// </summary>
public interface IAuditHierarchyBuilder
{
    IAuditEntityHierarchyBuilder<TEntity> Entity<TEntity>() where TEntity : class;
}

public interface IAuditEntityHierarchyBuilder<TEntity> where TEntity : class
{
    /// <summary>Declares <typeparamref name="TEntity"/> an Aggregate Root - audit graphs for it and its descendants are grouped under its own identity.</summary>
    IAuditEntityHierarchyBuilder<TEntity> IsRoot<TKey>(Expression<Func<TEntity, TKey>> idSelector);

    /// <summary>Declares <typeparamref name="TEntity"/> a child of <typeparamref name="TParent"/>, identified by the given foreign-key selector on <typeparamref name="TEntity"/>.</summary>
    IAuditEntityHierarchyBuilder<TEntity> BelongsTo<TParent>(Expression<Func<TEntity, object>> parentIdSelector)
        where TParent : class;
}
