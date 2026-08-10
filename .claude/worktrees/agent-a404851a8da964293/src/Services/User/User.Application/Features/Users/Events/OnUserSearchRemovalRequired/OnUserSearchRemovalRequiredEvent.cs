using NovaCore.BuildingBlock.Application.Abstractions.Events;

namespace NovaCore.User.Application.Features.Users.Events.OnUserSearchRemovalRequired;

/// <summary>Raised only when a UserProfile is deleted - the profile no longer exists in Postgres, so the reaction is a delete, not a rebuild.</summary>
public sealed record OnUserSearchRemovalRequiredEvent(Guid UserId, string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
