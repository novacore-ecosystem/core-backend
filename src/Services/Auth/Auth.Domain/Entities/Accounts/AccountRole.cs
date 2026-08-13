using Microsoft.AspNetCore.Identity;

using NovaCore.Auth.Domain.Entities.Roles;

namespace NovaCore.Auth.Domain.Entities.Accounts;

/// <summary>
/// Owned child of Account - a many-to-many join between Account and Role, with a surrogate Id
/// to allow for auditing and future lifecycle management. Exists so JWT issuance never has to join
/// across Account/Role/PermissionDefinition at login time.
/// </summary>
public class AccountRole : IdentityUserRole<Guid>, IEntity
{
    public virtual Account? Account { get; set; }
    public virtual Role? Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Must be public: ASP.NET Core Identity's UserStore requires TUserRole : IdentityUserRole<TKey>, new()
    // with an accessible parameterless constructor (it's instantiated via generic constraint from
    // Identity's own assembly). Controlled construction is still expected through Create() below.
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
