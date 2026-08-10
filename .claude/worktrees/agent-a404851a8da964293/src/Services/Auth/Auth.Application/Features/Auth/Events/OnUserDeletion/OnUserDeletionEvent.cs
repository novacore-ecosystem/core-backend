namespace NovaCore.Auth.Application.Features.Auth.Events.OnUserDeletion;

public sealed record OnUserDeletionEvent(
    string UserId,
    string Reason,
    string CorrelationId) : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
