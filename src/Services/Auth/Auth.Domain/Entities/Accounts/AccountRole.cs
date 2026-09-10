using Microsoft.AspNetCore.Identity;

using NovaCore.Auth.Domain.Entities.Roles;

namespace NovaCore.Auth.Domain.Entities.Accounts;

/// <summary>
/// A Role directly assigned to an Account.
/// </summary>
/// <remarks>
/// IAuditable and registered BelongsTo(Account) in Auth.Persistence's ConfigureAuditHierarchy,
/// same shape as AccountPosition - assigning/removing a Role is itself the business event, not
/// incidental mapping noise.
/// </remarks>
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
