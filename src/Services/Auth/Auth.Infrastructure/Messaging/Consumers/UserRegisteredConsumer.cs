using System.Text.Json;

using NovaCore.Auth.Application.Features.Auth.Commands.DispatchVerificationEmail;

using MediatR;

namespace NovaCore.Auth.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes UserRegisteredIntegrationEvent and triggers the same verification-email dispatch flow
/// Register's own request already calls synchronously - a resilience/observability path (retried
/// via Inbox if this never runs, e.g. Kafka outage during Register), not the sole trigger: the
/// resend cooldown is established synchronously by Register itself, so this consumer's dispatch is
/// commonly a no-op (the claim is already held). This only reaches the point of enqueuing
/// EmailVerificationRequestedIntegrationEvent - actually sending the email is Notification
/// Service's job (see NotificationTriggerConsumer), never Auth's. Thin by design: deserialize and
/// dispatch a Command, same shape as every other integration event consumer.
/// </summary>
public sealed class UserRegisteredConsumer(ISender sender) : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(UserRegisteredIntegrationEvent).ToLowerInvariant()
    ];

    public async Task HandleAsync(
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        var data = JsonSerializer.Deserialize<UserRegisteredIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize UserRegisteredIntegrationEvent");

        await sender.Send(new DispatchVerificationEmailCommand(data.Email), ct);
    }
}
