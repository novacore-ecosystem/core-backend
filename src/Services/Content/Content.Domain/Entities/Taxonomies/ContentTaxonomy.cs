namespace NovaCore.Content.Domain.Entities.Taxonomies;

/// <summary>
/// Aggregate root for a hierarchical taxonomy/classification node (category, topic, tag group,
/// ...). Content items reference taxonomy nodes through ContentTaxonomyAssignment, owned by
/// Content - this aggregate only models the classification tree itself.
/// </summary>
public sealed class ContentTaxonomy : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public ContentKey Key { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid? ParentId { get; private set; }
    public ContentTaxonomy? Parent { get; private set; }
    public ContentTypeStatus Status { get; private set; }

    public ICollection<ContentTaxonomy> Children { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ContentTaxonomy() { }

    public static ContentTaxonomy Create(
        ContentKey key,
        string name,
        string description,
        Guid? parentId = null,
        ContentTypeStatus status = ContentTypeStatus.Active)
    {
        ValidateName(name);

        return new ContentTaxonomy
        {
            Id = Guid.CreateVersion7(),
            Key = key,
            Name = name,
            Description = description,
            ParentId = parentId,
            Status = status,
        };
    }

    // ============================================================================
    // Details & lifecycle
    // ============================================================================

    #region Details & lifecycle

    public void UpdateDetails(string name, string description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    /// <summary>
    /// Moves this node under a new parent (or to root when null). Only guards against direct
    /// self-parenting - detecting a deeper ancestor cycle requires querying the full taxonomy
    /// tree, an Application-layer concern, same reasoning as ProductCategory.ChangeParent.
    /// </summary>
    public void ChangeParent(Guid? parentId)
    {
        if (parentId == Id)
            throw ExceptionFactory.InvalidState("A taxonomy node cannot be its own parent.");

        ParentId = parentId;
    }

    public void Activate()
    {
        Status = ContentTypeStatus.Active;
    }

    public void Deactivate()
    {
        Status = ContentTypeStatus.Inactive;
    }

    public void Archive()
    {
        Status = ContentTypeStatus.Archived;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Taxonomy name cannot be empty.");
    }

    #endregion
}
