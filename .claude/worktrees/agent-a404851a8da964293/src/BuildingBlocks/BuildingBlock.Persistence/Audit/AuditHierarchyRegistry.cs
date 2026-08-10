using System.Diagnostics.CodeAnalysis;

namespace NovaCore.BuildingBlock.Persistence.Audit;

/// <summary>
/// Built once, at startup, from whatever <c>ConfigureAuditHierarchy</c> registered. Validates and
/// resolves every type's <see cref="AuditEntityMetadata.RootType"/> eagerly here - a misconfigured
/// hierarchy (a <c>BelongsTo{TParent}</c> pointing at an unregistered type, or a cycle) fails fast
/// at startup instead of silently guessing or breaking deep inside a SaveChanges call.
/// </summary>
public sealed class AuditHierarchyRegistry : IAuditHierarchyRegistry
{
    private readonly Dictionary<Type, AuditEntityMetadata> _metadataByType;

    public AuditHierarchyRegistry(IReadOnlyDictionary<Type, AuditEntityMetadata> metadataByType)
    {
        _metadataByType = new Dictionary<Type, AuditEntityMetadata>(metadataByType);

        ResolveRootTypes();
    }

    public bool TryGetMetadata(Type entityType, [MaybeNullWhen(false)] out AuditEntityMetadata metadata)
        => _metadataByType.TryGetValue(entityType, out metadata);

    private void ResolveRootTypes()
    {
        foreach (var meta in _metadataByType.Values)
        {
            var visited = new HashSet<Type>();
            var current = meta;

            while (!current.IsRoot)
            {
                if (current.ParentType is null || !visited.Add(current.EntityType))
                    throw new InvalidOperationException(
                        $"Audit hierarchy for '{current.EntityType.Name}' does not resolve to a root. " +
                        "Every type registered via ConfigureAuditHierarchy must either call IsRoot() or " +
                        "have a BelongsTo<TParent> chain that eventually reaches a type registered with IsRoot().");

                if (!_metadataByType.TryGetValue(current.ParentType, out var parent))
                    throw new InvalidOperationException(
                        $"Audit hierarchy for '{current.EntityType.Name}' declares BelongsTo<{current.ParentType.Name}>, " +
                        $"but '{current.ParentType.Name}' was never registered via ConfigureAuditHierarchy.");

                current = parent;
            }

            meta.RootType = current.EntityType;
        }
    }
}
