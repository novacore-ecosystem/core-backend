using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Events;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.User;
using NovaCore.BuildingBlock.Messaging.Abstractions;

using NovaCore.User.Application.Features.Users.Events.OnUserSearchSyncRequired;

namespace NovaCore.User.Infrastructure.Messaging.Consumers;

/// <summary>User self-consumes its own integration events to keep the Search index in sync - see docs/reference/search.md.</summary>
public sealed class UserProfileCreatedSearchSyncConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<UserProfileCreatedSearchSyncConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(UserProfileCreatedIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message, IReadOnlyDictionary<string, string> headers, CancellationToken ct = default)
    {
        var integrationEvent = JsonSerializer.Deserialize<UserProfileCreatedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize UserProfileCreatedIntegrationEvent");

        logger.Information("Received UserProfileCreatedIntegrationEvent for UserId: {UserId}", integrationEvent.UserId);

        var @event = new OnUserSearchSyncRequiredEvent(integrationEvent.UserId, integrationEvent.CorrelationId);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information("Successfully processed UserProfileCreatedIntegrationEvent for UserId: {UserId}", integrationEvent.UserId);
    }
}
