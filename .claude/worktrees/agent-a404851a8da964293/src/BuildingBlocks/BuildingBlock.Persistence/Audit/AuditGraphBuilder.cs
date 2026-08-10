using NovaCore.BuildingBlock.Contract.Events.Audit;

namespace NovaCore.BuildingBlock.Persistence.Audit;

/// <summary>
/// Pure, provider-agnostic algorithm: turns a flat set of tracked entities into one <see cref="AuditGraphResult"/>
/// per distinct Aggregate Root instance - never one per entity, never merging two different root
/// instances together. Ancestor paths are resolved dynamically per changed entity by walking
/// <see cref="AuditEntityMetadata.ParentType"/>/<see cref="AuditTrackedEntity.ParentEntityId"/>
/// via the registry - developers only ever configure a direct parent, the full chain up to the
/// root falls out of that automatically. Structural (unchanged) ancestors are kept in the
/// resulting tree with an empty <c>Changes</c> collection purely so a consumer can always walk
/// from any changed descendant back up to its root without losing the hierarchy in between.
/// </summary>
public static class AuditGraphBuilder
{
    public static IReadOnlyList<AuditGraphResult> Build(
        IReadOnlyCollection<AuditTrackedEntity> entities,
        IAuditHierarchyRegistry registry)
    {
        if (entities.Count == 0)
            return [];

        var byKey = new Dictionary<(Type EntityType, string EntityId), AuditTrackedEntity>();
        foreach (var entity in entities)
            byKey[(entity.EntityType, entity.EntityId)] = entity;

        var changed = entities.Where(e => e.HasChanges).ToArray();
        if (changed.Length == 0)
            return [];

        var rootBuilders = new Dictionary<(Type, string), AuditNodeBuilder>();

        foreach (var leaf in changed)
        {
            var chainRootToLeaf = ResolveChain(leaf, byKey, registry);
            Merge(rootBuilders, chainRootToLeaf);
        }

        return [.. rootBuilders.Select(kvp => new AuditGraphResult(
            kvp.Key.Item1.Name,
            kvp.Key.Item2,
            kvp.Value.ToNode(depth: 0, parentNodeId: null)))];
    }

    private static List<AuditTrackedEntity> ResolveChain(
        AuditTrackedEntity leaf,
        IReadOnlyDictionary<(Type, string), AuditTrackedEntity> byKey,
        IAuditHierarchyRegistry registry)
    {
        var chain = new List<AuditTrackedEntity> { leaf };
        var current = leaf;

        while (registry.TryGetMetadata(current.EntityType, out var meta) && meta.ParentType is not null)
        {
            // Missing FK, or the parent instance isn't tracked in this unit of work (e.g. only a
            // leaf was loaded on its own, without its aggregate root) - best effort: stop here and
            // treat `current` as the effective root of this branch rather than guessing further.
            if (current.ParentEntityId is null)
                break;

            if (!byKey.TryGetValue((meta.ParentType, current.ParentEntityId), out var parent))
                break;

            chain.Add(parent);
            current = parent;
        }

        chain.Reverse();
        return chain;
    }

    private static void Merge(Dictionary<(Type, string), AuditNodeBuilder> rootBuilders, List<AuditTrackedEntity> chainRootToLeaf)
    {
        var rootEntity = chainRootToLeaf[0];
        var rootKey = (rootEntity.EntityType, rootEntity.EntityId);

        if (!rootBuilders.TryGetValue(rootKey, out var builder))
        {
            builder = new AuditNodeBuilder(rootEntity);
            rootBuilders[rootKey] = builder;
        }
        else
        {
            builder.MergeEntity(rootEntity);
        }

        for (var i = 1; i < chainRootToLeaf.Count; i++)
            builder = builder.GetOrAddChild(chainRootToLeaf[i]);
    }

    /// <summary>Mutable scratch structure used only while assembling a graph - converted to the immutable, wire-shaped <see cref="AuditNode"/> tree once, via <see cref="ToNode"/>.</summary>
    private sealed class AuditNodeBuilder(AuditTrackedEntity entity)
    {
        private AuditTrackedEntity _entity = entity;
        private readonly Dictionary<(Type, string), AuditNodeBuilder> _children = [];

        public void MergeEntity(AuditTrackedEntity updated)
        {
            if (updated.HasChanges && !_entity.HasChanges)
                _entity = updated;
        }

        public AuditNodeBuilder GetOrAddChild(AuditTrackedEntity childEntity)
        {
            var key = (childEntity.EntityType, childEntity.EntityId);

            if (!_children.TryGetValue(key, out var child))
            {
                child = new AuditNodeBuilder(childEntity);
                _children[key] = child;
            }
            else
            {
                child.MergeEntity(childEntity);
            }

            return child;
        }

        public AuditNode ToNode(int depth, Guid? parentNodeId)
        {
            var nodeId = Guid.NewGuid();
            var children = _children.Values
                .Select(c => c.ToNode(depth + 1, nodeId))
                .ToArray();

            return new AuditNode(
                nodeId, parentNodeId, depth,
                _entity.EntityType.Name, _entity.EntityId, _entity.Action, _entity.Changes, children);
        }
    }
}
