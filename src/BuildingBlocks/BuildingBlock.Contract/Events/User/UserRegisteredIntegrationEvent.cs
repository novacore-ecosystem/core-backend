namespace NovaCore.BuildingBlock.Contract.Events.User;

/// <summary>
/// Raised by Auth's Register flow once the account has been created - triggers the same
/// verification-email dispatch flow ResendEmail uses, kept as an event (rather than an in-process
/// call only) so retrying/observing the post-registration email dispatch doesn't require
/// re-running Register itself.
/// </summary>
public sealed record UserRegisteredIntegrationEvent(
    string AccountId,
    string Email,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; } = nameof(UserRegisteredIntegrationEvent);
    public DateTime PublishedAt { get; } = DateTime.UtcNow;
}
