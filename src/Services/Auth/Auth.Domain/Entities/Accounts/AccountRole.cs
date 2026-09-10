using Microsoft.AspNetCore.Identity;

using NovaCore.Auth.Domain.Entities.Roles;

namespace NovaCore.Auth.Domain.Entities.Accounts;

/// <summary>IAuditable - unlike PositionRole, assigning/removing a Role on an Account IS the
/// business event this feature needs a trail for, not incidental mapping noise. Registered as
/// BelongsTo(Account) in Auth.Persistence's ConfigureAuditHierarchy, same shape as
/// AccountPosition.</summary>
public class AccountRole : IdentityUserRole<Guid>, IEntity, IAuditable
{
    public virtual Account? Account { get; set; }
    public virtual Role? Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public AccountRole() { }

    internal static AccountRole Create(Guid accountId, Guid roleId)
    {
        return new AccountRole
        {
            UserId = accountId,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Track()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
