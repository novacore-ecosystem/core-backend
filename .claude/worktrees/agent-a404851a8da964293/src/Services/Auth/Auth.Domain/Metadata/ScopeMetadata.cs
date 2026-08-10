using NovaCore.BuildingBlock.Domain.Metadata;

namespace NovaCore.Auth.Domain.Metadata;

/// <summary>Extensible, strongly-typed metadata bag for a Scope. No fields defined yet -
/// future scope-specific settings are added here as [Metadata]-attributed properties,
/// following ProductMetadata's shape, rather than as loose columns on Scope itself.</summary>
public sealed class ScopeMetadata : MetadataBase
{
}
