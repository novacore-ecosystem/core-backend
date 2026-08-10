using NovaCore.BuildingBlock.Domain.Metadata;

namespace NovaCore.Auth.Domain.Metadata;

/// <summary>Extensible, strongly-typed metadata bag for a Tenant. No fields defined yet -
/// future tenant-specific settings are added here as [Metadata]-attributed properties,
/// following ProductMetadata's shape, rather than as loose columns on Tenant itself.</summary>
public sealed class TenantMetadata : MetadataBase
{
}
