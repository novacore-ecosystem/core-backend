using NovaCore.Auth.Domain.Entities.Apps;
using NovaCore.Auth.Domain.Entities.Permissions;

namespace NovaCore.Auth.Domain.Entities.Registrations;

/// <summary>
/// A permission automatically granted directly to every Account that self-registers into a
/// given App, for a given Tenant - the (Tenant, App)-scoped source RegisterHandler consults
/// alongside <see cref="RegistrationDefaultRole"/> when provisioning a new account's defaults.
/// </summary>
/// <remarks>
/// Own audit root (see Auth.Persistence's ConfigureAuditHierarchy), not BelongsTo - like
/// PermissionGrant, no single owning aggregate applies. TenantId is a genuine identity
/// component here (the same App+PermissionDefinition pairing is a legitimately distinct row per
/// tenant), so this keeps a surrogate Id plus a unique composite index rather than a composite
/// primary key, mirroring PermissionGrant's own shape.
/// </remarks>
public sealed class RegistrationDefaultPermission : BaseEntity<Guid>, ITenantEntity, IAuditable
{
    public Guid AppId { get; init; }
    public App App { get; init; } = default!;
    public Guid PermissionDefinitionId { get; init; }
    public PermissionDefinition PermissionDefinition { get; init; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private RegistrationDefaultPermission() { }

    public static RegistrationDefaultPermission Create(Guid appId, Guid permissionDefinitionId)
    {
        return new RegistrationDefaultPermission
        {
            Id = Guid.CreateVersion7(),
            AppId = appId,
            PermissionDefinitionId = permissionDefinitionId,
        };
    }
}
