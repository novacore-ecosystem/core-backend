namespace NovaCore.BuildingBlock.Domain.Attributes;

/// <summary>
/// Marks a property as excluded from audit comparison. The change-tracking pipeline never
/// includes properties carrying this attribute in a generated audit change snapshot, even
/// for an entity that implements <see cref="Abstractions.IAuditable"/> - typical candidates
/// are bookkeeping fields such as UpdatedAt, LastAccessedAt, RetryCount, or CacheVersion.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class AuditIgnoreAttribute : Attribute
{
}
