namespace NovaCore.BuildingBlock.Persistence.Audit;

/// <summary>
/// Resolved hierarchy metadata for one entity type, produced by <see cref="AuditHierarchyBuilder"/>
/// and looked up through <see cref="IAuditHierarchyRegistry"/>. Never guessed at runtime - every
/// field here comes directly from an explicit <c>IsRoot()</c>/<c>BelongsTo{TParent}()</c>
/// registration, or (for <see cref="RootType"/>) is derived once, at startup, by walking those
/// explicit registrations to their root.
/// </summary>
public sealed class AuditEntityMetadata
{
    public required Type EntityType { get; init; }

    public bool IsRoot { get; init; }

    /// <summary>Direct parent type, from <c>BelongsTo{TParent}()</c>. Null for root types.</summary>
    public Type? ParentType { get; init; }

    /// <summary>Reads the FK value on an instance of <see cref="EntityType"/> pointing at its <see cref="ParentType"/> instance. Null for root types.</summary>
    public Func<object, object?>? ParentIdAccessor { get; init; }

    /// <summary>Reads an instance's own identity, as registered via <c>IsRoot(idSelector)</c>. Only set for root registrations - non-root identity extraction is a provider concern (e.g. EF's own primary-key metadata).</summary>
    public Func<object, object?>? IdAccessor { get; init; }

    /// <summary>The ultimate ancestor type for this entity type, resolved once by <see cref="AuditHierarchyRegistry"/> at construction time.</summary>
    public Type RootType { get; internal set; } = null!;
}
