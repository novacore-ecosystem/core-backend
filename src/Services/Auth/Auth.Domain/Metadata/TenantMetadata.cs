using NovaCore.BuildingBlock.Domain.Metadata;

namespace NovaCore.Auth.Domain.Metadata;

/// <summary>Extensible, strongly-typed metadata bag for a Tenant. Future tenant-specific settings
/// are added here as [Metadata]-attributed properties, following ProductMetadata's shape, rather
/// than as loose columns on Tenant itself.</summary>
public sealed class TenantMetadata : MetadataBase
{
    /// <summary>Opts this tenant into permission-boundary enforcement - while false (the
    /// default), the tenant's Role/User permission grants are unrestricted (backward-compatible
    /// with every tenant that predates this feature). Once true, only permission keys ROOT has
    /// granted to this tenant (PermissionGrant, ProviderName = Tenant) may be newly granted to a
    /// Role or Account within it - see AccountAuthorizationGuard.EnsureWithinTenantBoundary.</summary>
    [Metadata]
    public bool PermissionBoundaryEnabled { get => Get<bool>(); set => Set(value); }
}
