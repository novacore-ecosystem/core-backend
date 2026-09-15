namespace NovaCore.BuildingBlock.Contract.Events.User;

/// <summary>
/// Raised by Auth's ForgotPassword flow once a reset token has been generated - carries
/// everything Notification needs to render and send the reset email, since Notification has no
/// account/email lookup of its own. Never logged at Information level - ResetLink embeds a live,
/// single-use credential.
/// </summary>
public sealed record PasswordResetRequestedIntegrationEvent(
    string AccountId,
    string Email,
    string ResetLink,
    int ExpiresInMinutes,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; } = nameof(PasswordResetRequestedIntegrationEvent);
    public DateTime PublishedAt { get; } = DateTime.UtcNow;
}
