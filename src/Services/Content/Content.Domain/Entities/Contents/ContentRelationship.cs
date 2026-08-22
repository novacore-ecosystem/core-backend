namespace NovaCore.Content.Domain.Entities.Contents;

/// <summary>
/// A directed relationship from a Content to another resource - another Content, a Product, a
/// User, an Asset, or any future resource type. TargetType is a plain discriminator string
/// rather than a set of nullable typed FKs, since the target may live in a different bounded
/// context this service must never take foreign-key ownership over.
/// </summary>
public sealed class ContentRelationship : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid SourceContentId { get; private set; }
    public Content SourceContent { get; private set; } = default!;
    public string TargetType { get; private set; } = string.Empty;
    public Guid TargetId { get; private set; }
    public ContentRelationshipType RelationshipType { get; private set; }
    public string? Metadata { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ContentRelationship() { }

    internal static ContentRelationship Create(
        Guid sourceContentId,
        string targetType,
        Guid targetId,
        ContentRelationshipType relationshipType,
        string? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(targetType))
            throw ExceptionFactory.RequiredField("Content relationship target type cannot be empty.");

        return new ContentRelationship
        {
            Id = Guid.CreateVersion7(),
            SourceContentId = sourceContentId,
            TargetType = targetType,
            TargetId = targetId,
            RelationshipType = relationshipType,
            Metadata = metadata,
        };
    }
}
