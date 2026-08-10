namespace NovaCore.Audit.Domain.Entities;

/// <summary>
/// One node of a persisted audit graph - the Mongo-embedded mirror of
/// NovaCore.BuildingBlock.Contract.Events.Audit.AuditNode. Kept as its own type (not a shared reference to
/// the Contract record) so NovaCore.Audit.Domain stays free of a Contract dependency, same reasoning as
/// every other entity in this project. Persisted as a real nested document (not a serialized
/// string), so MongoDB can query into <see cref="EntityType"/>/<see cref="EntityId"/> at any
/// depth if ever needed - <see cref="Children"/> is a true recursive hierarchy, not a flat list.
/// </summary>
public sealed class AuditTrailNode
{
    public Guid NodeId { get; private set; }
    public Guid? ParentNodeId { get; private set; }
    public int Depth { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public IReadOnlyCollection<AuditTrailFieldChange> Changes { get; private set; } = [];
    public IReadOnlyCollection<AuditTrailNode> Children { get; private set; } = [];

    private AuditTrailNode() { }

    public static AuditTrailNode Create(
        Guid nodeId,
        Guid? parentNodeId,
        int depth,
        string entityType,
        string entityId,
        string action,
        IReadOnlyCollection<AuditTrailFieldChange> changes,
        IReadOnlyCollection<AuditTrailNode> children)
    {
        return new AuditTrailNode
        {
            NodeId = nodeId,
            ParentNodeId = parentNodeId,
            Depth = depth,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Changes = changes,
            Children = children,
        };
    }
}
