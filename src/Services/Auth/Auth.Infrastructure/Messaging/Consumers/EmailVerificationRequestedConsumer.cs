using System.Text.Json;

using NovaCore.Auth.Application.Features.Auth.Events.OnEmailVerificationRequested;

namespace NovaCore.Auth.Infrastructure.Messaging.Consumers;

/// <summary>
/// Dedicated consumer for EmailVerificationRequestedIntegrationEvent - deliberately separate from
/// Notification's NotificationTriggerConsumer since this isn't a user notification, it's Auth's
/// own transactional email. Thin by design: deserialize and hand off to the internal event
/// dispatcher, same shape as AuthenticationSucceededConsumer.
/// </summary>
public sealed class EmailVerificationRequestedConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<EmailVerificationRequestedConsumer> logger) : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(EmailVerificationRequestedIntegrationEvent).ToLowerInvariant()
    ];

    public async Task HandleAsync(
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        var data = JsonSerializer.Deserialize<EmailVerificationRequestedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize EmailVerificationRequestedIntegrationEvent");

        var @event = new OnEmailVerificationRequestedEvent(
            Guid.Parse(data.AccountId),
            data.Email,
            data.VerificationLink,
            data.CorrelationId);

        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information("Processed EmailVerificationRequestedIntegrationEvent for AccountId: {AccountId}", data.AccountId);
    }
}
