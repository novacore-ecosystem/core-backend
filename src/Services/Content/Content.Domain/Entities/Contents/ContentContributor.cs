namespace NovaCore.Content.Domain.Entities.Contents;

/// <summary>
/// Records one user's participation in authoring a Content. User identity is owned by
/// UserService - this holds only a reference id plus the authoring role, which is a content
/// participation concept, not a system authorization role.
/// </summary>
public sealed class ContentContributor : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid ContentId { get; private set; }
    public Content Content { get; private set; } = default!;
    public Guid UserId { get; private set; }
    public ContributorRole Role { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ContentContributor() { }

    internal static ContentContributor Create(Guid contentId, Guid userId, ContributorRole role)
    {
        return new ContentContributor
        {
            Id = Guid.CreateVersion7(),
            ContentId = contentId,
            UserId = userId,
            Role = role,
        };
    }

    internal void ChangeRole(ContributorRole role)
    {
        Role = role;
    }
}
