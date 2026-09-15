namespace NovaCore.Auth.Application.Features.Auth.Events.OnEmailVerificationRequested;

public sealed record OnEmailVerificationRequestedEvent(
    Guid AccountId,
    string Email,
    string VerificationLink,
    string CorrelationId) : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
