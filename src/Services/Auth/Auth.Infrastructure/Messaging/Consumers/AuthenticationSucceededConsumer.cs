using System.Text.Json;

using NovaCore.Auth.Application.Features.Auth.Events.OnAuthenticationSucceeded;

namespace NovaCore.Auth.Infrastructure.Messaging.Consumers;

public sealed class AuthenticationSucceededConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<AuthenticationSucceededConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(AuthenticationSucceededIntegrationEvent).ToLowerInvariant()
    ];

    public async Task HandleAsync(
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        var data = JsonSerializer.Deserialize<AuthenticationSucceededIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize AuthenticationSucceededIntegrationEvent");

        logger.Information(
            "Received AuthenticationSucceededIntegrationEvent for AccountId: {AccountId}",
            data.AccountId);

        var @event = new OnAuthenticationSucceededEvent(
            Guid.Parse(data.AccountId),
            string.IsNullOrEmpty(data.AppId) ? null : Guid.Parse(data.AppId),
            Guid.Parse(data.TenantId),
            data.AuthenticationType,
            Guid.Parse(data.JwtId),
            data.IpAddress,
            data.CorrelationId);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information(
            "Successfully processed AuthenticationSucceededIntegrationEvent for AccountId: {AccountId}",
            data.AccountId);
    }
}
