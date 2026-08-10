using NovaCore.BuildingBlock.Application.Abstractions.Events;

namespace NovaCore.User.Application.Features.Users.Events.OnUserSearchSyncRequired;

/// <summary>
/// Raised whenever a UserProfile is created or updated (via self-consumption of
/// UserProfileCreatedIntegrationEvent/UserProfileUpdatedIntegrationEvent) or synchronously by
/// OnUserInitiatedHandler (Auth's self-registration path) - rebuilding the whole document from
/// current Postgres state is simpler and more correct than threading partial updates through
/// multiple event shapes. See docs/reference/search.md.
/// </summary>
public sealed record OnUserSearchSyncRequiredEvent(Guid UserId, string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
