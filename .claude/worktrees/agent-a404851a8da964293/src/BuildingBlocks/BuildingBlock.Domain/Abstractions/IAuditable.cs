namespace NovaCore.BuildingBlock.Domain.Abstractions;

/// <summary>
/// Opt-in marker for entities that should participate in audit tracking. The change-tracking
/// pipeline ignores any entity that does not implement this interface, filtering it out before
/// any property-level comparison happens.
/// </summary>
public interface IAuditable
{
}
