using System.Text.Json;

using NovaCore.Auth.Application.Features.Auth.Events.OnUserCreated;

namespace NovaCore.Auth.Infrastructure.Messaging.Consumers;

public sealed class UserCreatedIntegrationEventConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<UserCreatedIntegrationEventConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(UserProfileCreatedIntegrationEvent).ToLowerInvariant()
    ];

    public async Task HandleAsync(
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        var data = JsonSerializer.Deserialize<UserProfileCreatedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize UserCreatedIntegrationEvent");

        logger.Information(
            "Received UserCreatedIntegrationEvent for UserId: {UserId}",
            data.UserId);

        var @event = new OnUserCreatedEvent(
            data.UserId.ToString(),
            data.Email,
            data.UserName,
            data.FirstName,
            data.MiddleName,
            data.LastName,
            data.Roles,
            data.TempPassword,
            data.CorrelationId);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information(
            "Successfully processed UserCreatedIntegrationEvent for UserId: {UserId}",
            data.UserId);
    }
}
