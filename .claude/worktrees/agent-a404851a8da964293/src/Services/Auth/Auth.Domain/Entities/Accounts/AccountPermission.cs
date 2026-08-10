namespace NovaCore.Auth.Domain.Entities.Accounts;

/// <summary>
/// Owned child of Account - a denormalized cache of one effective permission, rebuilt whenever
/// role assignment changes via Account.RefreshPermissionSnapshot().
/// Exists so JWT issuance never has to join across Role/PermissionDefinition at login time.
/// </summary>
public sealed class AccountPermission : BaseEntity<Guid>, ITenantEntity
{
    public Guid AccountId { get; private set; }
    public Account Account { get; private set; } = default!;
    public PermissionKey PermissionKey { get; private set; } = null!;
    public Guid SourceRoleId { get; private set; }
    public DateTime CachedAt { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private AccountPermission() { }

    internal static AccountPermission Create(Guid accountId, PermissionKey permissionKey, Guid sourceRoleId)
    {
        return new AccountPermission
        {
            Id = Guid.CreateVersion7(),
            AccountId = accountId,
            PermissionKey = permissionKey,
            SourceRoleId = sourceRoleId,
            CachedAt = DateTime.UtcNow,
        };
    }

    public void Refresh()
    {
        CachedAt = DateTime.UtcNow;
    }
}
