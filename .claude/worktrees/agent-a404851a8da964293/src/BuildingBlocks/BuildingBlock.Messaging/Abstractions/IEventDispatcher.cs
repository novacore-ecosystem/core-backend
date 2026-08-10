using NovaCore.BuildingBlock.Contract.Events;

namespace NovaCore.BuildingBlock.Messaging.Abstractions;

public interface IEventDispatcher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class, IIntegrationEvent;
}
