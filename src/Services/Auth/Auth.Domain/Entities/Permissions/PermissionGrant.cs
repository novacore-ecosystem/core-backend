using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.Auth.Domain.Entities.Permissions;

/// <summary>
/// Centralized permission assignment - replaces the former Role-only RolePermission join.
/// Deliberately generic: this entity has no FK/navigation back to Role or any other specific
/// provider type, only the (ProviderName, ProviderKey) pair every provider category shares. Today
/// the only wired grant path is ProviderName == Role, ProviderKey == that Role's Id (see
/// RoleWriteService/PermissionGrantService) - a future direct grant to a User/Client/Guest reuses
/// this exact table with a different ProviderName, never a new *_permissions table. Server-side
/// provider-applicability validation (against PermissionRegistry.Instance) happens in
/// PermissionGrantService before a grant is ever persisted - this entity itself trusts its caller,
/// same as RolePermission did.
/// </summary>
public sealed class PermissionGrant : BaseEntity<Guid>, ITenantEntity
{
    public Guid PermissionDefinitionId { get; init; }
    public PermissionDefinition PermissionDefinition { get; init; } = default!;
    public PermissionProviderName ProviderName { get; init; }
    public string ProviderKey { get; init; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private PermissionGrant() { }

    /// <summary>Public, unlike RolePermission's former internal factory - PermissionGrant is
    /// created from the cross-provider Persistence-layer PermissionGrantService, not from a
    /// Domain aggregate method on the provider it happens to represent today.</summary>
    public static PermissionGrant Create(Guid permissionDefinitionId, PermissionProviderName providerName, string providerKey)
    {
        if (!providerName.IsSingleValue())
            throw ExceptionFactory.InvalidRange(
                $"PermissionGrant.ProviderName must be exactly one provider category, got \"{providerName}\".");

        if (string.IsNullOrWhiteSpace(providerKey))
            throw ExceptionFactory.RequiredField("PermissionGrant.ProviderKey cannot be empty.");

        return new PermissionGrant
        {
            Id = Guid.CreateVersion7(),
            PermissionDefinitionId = permissionDefinitionId,
            ProviderName = providerName,
            ProviderKey = providerKey,
        };
    }
}
