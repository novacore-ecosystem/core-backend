using System.Diagnostics;
using System.Text;
using System.Text.Json;

using NovaCore.BuildingBlock.Contract.Events;
using NovaCore.BuildingBlock.Messaging.Abstractions;
using NovaCore.BuildingBlock.Messaging.Kafka.Configuration;
using NovaCore.BuildingBlock.Messaging.Kafka.Tracing;

using KafkaFlow;
using KafkaFlow.Producers;

namespace NovaCore.BuildingBlock.Messaging.Kafka.Publishers;

public sealed class KafkaFlowEventPublisher(
    IProducerAccessor producerAccessor) : IEventPublisher
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class, IIntegrationEvent
    {
        var producer = producerAccessor.GetProducer(KafkaFlowDefaults.ProducerName);
        var topic = GenerateTopicName(@event);
        var value = JsonSerializer.SerializeToUtf8Bytes(@event);

        using var activity = KafkaTracing.StartProducerActivity(topic);

        var headers = new MessageHeaders
        {
            { "event-type", Encoding.UTF8.GetBytes(typeof(TEvent).Name) },
            { "correlation-id", Encoding.UTF8.GetBytes(@event.CorrelationId) },
        };
        KafkaTracing.InjectTraceContext(headers);

        try
        {
            await producer.ProduceAsync(topic, @event.CorrelationId, value, headers);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

    public async Task PublishBatchAsync<TEvent>(
        IEnumerable<TEvent> events,
        CancellationToken ct = default) where TEvent : class, IIntegrationEvent
    {
        foreach (var @event in events)
        {
            await PublishAsync(@event, ct);
        }
    }

    private static string GenerateTopicName<TEvent>(TEvent @event) where TEvent : IIntegrationEvent
    {
        return @event.EventType.ToLowerInvariant();
    }
}
