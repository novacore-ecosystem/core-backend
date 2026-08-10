namespace NovaCore.BuildingBlock.Contract.Events;

public interface IIntegrationEvent
{
    string CorrelationId { get; }
    string EventType { get; }
    DateTime PublishedAt { get; }
}
