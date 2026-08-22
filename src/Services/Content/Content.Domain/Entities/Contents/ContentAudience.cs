namespace NovaCore.Content.Domain.Entities.Contents;

/// <summary>
/// One audience-targeting rule for a Content. AudienceReferenceId is null for the two
/// blanket audience types (AllUsers/AuthenticatedUsers) and set to the referenced
/// Organization/Group/Role id otherwise - the reference is external, resolved by the
/// authorization system, never duplicated here.
/// </summary>
public sealed class ContentAudience : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid ContentId { get; private set; }
    public Content Content { get; private set; } = default!;
    public AudienceType AudienceType { get; private set; }
    public Guid? AudienceReferenceId { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ContentAudience() { }

    internal static ContentAudience Create(Guid contentId, AudienceType audienceType, Guid? audienceReferenceId = null)
    {
        var requiresReference = audienceType is AudienceType.Organization or AudienceType.Group or AudienceType.Role;
        if (requiresReference && audienceReferenceId is null)
            throw ExceptionFactory.RequiredField($"Audience type '{audienceType}' requires a reference id.");

        if (!requiresReference && audienceReferenceId is not null)
            throw ExceptionFactory.InvalidState($"Audience type '{audienceType}' cannot carry a reference id.");

        return new ContentAudience
        {
            Id = Guid.CreateVersion7(),
            ContentId = contentId,
            AudienceType = audienceType,
            AudienceReferenceId = audienceReferenceId,
        };
    }
}
