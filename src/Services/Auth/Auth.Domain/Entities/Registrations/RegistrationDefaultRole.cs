using NovaCore.Auth.Domain.Entities.Apps;
using NovaCore.Auth.Domain.Entities.Roles;

namespace NovaCore.Auth.Domain.Entities.Registrations;

/// <summary>
/// A Role automatically granted to every Account that self-registers into a given App, for a
/// given Tenant - the (Tenant, App)-scoped replacement for the former hard-seeded "User" Role
/// RegisterHandler used to require.
/// </summary>
/// <remarks>
/// Own audit root (see Auth.Persistence's ConfigureAuditHierarchy), not BelongsTo - like
/// PermissionGrant, no single owning aggregate applies (neither Tenant nor App owns this
/// pairing). TenantId is a genuine identity component here (the same App+Role pairing is a
/// legitimately distinct row per tenant), so - like PermissionGrant - this keeps a surrogate Id
/// plus a unique composite index rather than a composite primary key.
/// </remarks>
public sealed class RegistrationDefaultRole : BaseEntity<Guid>, ITenantEntity, IAuditable
{
    public Guid AppId { get; init; }
    public App App { get; init; } = default!;
    public Guid RoleId { get; init; }
    public Role Role { get; init; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private RegistrationDefaultRole() { }

    public static RegistrationDefaultRole Create(Guid appId, Guid roleId)
    {
        return new RegistrationDefaultRole
        {
            Id = Guid.CreateVersion7(),
            AppId = appId,
            RoleId = roleId,
        };
    }
}
