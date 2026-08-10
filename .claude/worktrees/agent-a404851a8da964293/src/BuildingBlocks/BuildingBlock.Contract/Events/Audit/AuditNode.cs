namespace NovaCore.BuildingBlock.Contract.Events.Audit;

/// <summary>
/// One node in an audit graph - one tracked entity instance within an aggregate. Children are a
/// real hierarchy (not a flat list), so a consumer can walk the tree in either direction without
/// re-deriving structure: down via <see cref="Children"/>, up via <see cref="ParentNodeId"/>.
/// A node with an empty <see cref="Changes"/> collection is a structural pass-through - it didn't
/// change itself, but sits on the path between the root and a descendant that did (e.g. an
/// untouched OrderItem carrying a changed Discount), so traversal never loses the hierarchy.
/// </summary>
public sealed record AuditNode(
    Guid NodeId,
    Guid? ParentNodeId,
    int Depth,
    string EntityType,
    string EntityId,
    AuditAction Action,
    IReadOnlyCollection<AuditFieldChange> Changes,
    IReadOnlyCollection<AuditNode> Children);
