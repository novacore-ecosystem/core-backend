using NovaCore.BuildingBlock.Contract.Events;

namespace NovaCore.BuildingBlock.Messaging.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : class, IIntegrationEvent;

    Task PublishBatchAsync<T>(IEnumerable<T> events, CancellationToken cancellationToken = default)
        where T : class, IIntegrationEvent;
}
