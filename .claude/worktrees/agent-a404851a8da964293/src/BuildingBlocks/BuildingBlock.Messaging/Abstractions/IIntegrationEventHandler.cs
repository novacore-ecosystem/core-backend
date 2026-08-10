using NovaCore.BuildingBlock.Contract.Events;

namespace NovaCore.BuildingBlock.Messaging.Abstractions;

public interface IIntegrationEventHandler<TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}
