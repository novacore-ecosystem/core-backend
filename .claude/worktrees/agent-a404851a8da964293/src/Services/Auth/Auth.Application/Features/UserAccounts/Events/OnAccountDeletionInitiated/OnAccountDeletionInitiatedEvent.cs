namespace NovaCore.Auth.Application.Features.UserAccounts.Events.OnAccountDeletionInitiated;

public sealed record OnAccountDeletionInitiatedEvent(
    Guid AccountId) : IInternalEvent
{
    public string CorrelationId { get; } = string.Empty;
    public DateTime OccurredAt { get; }
};
