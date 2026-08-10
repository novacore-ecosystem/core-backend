using System.Text.Json;

using NovaCore.Auth.Application.Features.UserAccounts.Events.OnAccountDeletionInitiated;

namespace NovaCore.Auth.Infrastructure.Messaging.Consumers;

public sealed class UserAccountDeletionIntegrationEventConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<UserAccountDeletionIntegrationEventConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(UserDeletionIntegrationEvent).ToLowerInvariant()
    ];

    public async Task HandleAsync(
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        // Deliberately not caught here: the Inbox attempt executor (see
        // NovaCore.BuildingBlock.Infrastructure.Messaging.InboxAttemptExecutor) needs the exception to
        // propagate so it can record the failure and schedule a retry. Swallowing it here would
        // make every attempt look like a success and the message would never be retried.
        var data = JsonSerializer.Deserialize<UserDeletionIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize UserAccountDeletionIntegrationEvent");

        logger.Information(
            "Received UserAccountDeletionIntegrationEvent for UserId: {UserId}",
            data.UserId);

        var @event = new OnAccountDeletionInitiatedEvent(Guid.Parse(data.UserId));
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information(
            "Successfully processed UserAccountDeletionIntegrationEvent for UserId: {UserId}",
            data.UserId);
    }
}
