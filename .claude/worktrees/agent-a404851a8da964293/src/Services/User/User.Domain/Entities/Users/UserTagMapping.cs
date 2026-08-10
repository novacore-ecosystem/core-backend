using NovaCore.User.Domain.Entities.Tags;

namespace NovaCore.User.Domain.Entities.Users;

/// <summary>
/// Explicit many-to-many join entity between User and UserTag - User and UserTag are
/// independent aggregate roots, so this row (not a raw id collection) is how User references a
/// tag without holding an object reference to another root. Owned by User (no independent
/// identity, no repository of its own).
/// </summary>
public sealed class UserTagMapping : BaseEntity, ITenantEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public Guid TagId { get; private set; }
    public UserTag Tag { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private UserTagMapping() { }

    internal static UserTagMapping Create(Guid userId, Guid tagId)
    {
        return new UserTagMapping
        {
            UserId = userId,
            TagId = tagId,
        };
    }
}
