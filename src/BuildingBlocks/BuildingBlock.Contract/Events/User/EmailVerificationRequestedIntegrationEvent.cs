namespace NovaCore.BuildingBlock.Contract.Events.User;

/// <summary>
/// Raised by Auth's Register flow once an email-confirmation token has been generated - carries
/// everything Notification needs to render and send the verification email.
/// </summary>
public sealed record EmailVerificationRequestedIntegrationEvent(
    string AccountId,
    string Email,
    string VerificationLink,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; } = nameof(EmailVerificationRequestedIntegrationEvent);
    public DateTime PublishedAt { get; } = DateTime.UtcNow;
}
