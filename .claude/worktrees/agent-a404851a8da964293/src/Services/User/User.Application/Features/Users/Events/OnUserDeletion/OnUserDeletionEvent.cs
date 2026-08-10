using NovaCore.BuildingBlock.Application.Abstractions.Events;

namespace NovaCore.User.Application.Features.Users.Events.OnUserDeletion;

public record OnUserDeletionEvent(Guid Id) : IInternalEvent
{
    public string CorrelationId { get; } = string.Empty;
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}