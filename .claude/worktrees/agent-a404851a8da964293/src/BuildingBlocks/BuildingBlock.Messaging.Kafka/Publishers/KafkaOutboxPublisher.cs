using System.Diagnostics;
using System.Text;

using NovaCore.BuildingBlock.Messaging.Abstractions;
using NovaCore.BuildingBlock.Messaging.Kafka.Configuration;
using NovaCore.BuildingBlock.Messaging.Kafka.Tracing;

using KafkaFlow;
using KafkaFlow.Producers;

namespace NovaCore.BuildingBlock.Messaging.Kafka.Publishers;

/// <summary>
/// Kafka-based implementation of IOutboxPublisher.
/// Publishes outbox messages with message-id header for Inbox deduplication.
/// </summary>
public sealed class KafkaOutboxPublisher(
    IProducerAccessor producerAccessor) : IOutboxPublisher
{
    public async Task PublishOutboxMessageAsync(
        Guid messageId,
        string topic,
        string payload,
        string eventType,
        string correlationId,
        string? actorId,
        string actorType,
        CancellationToken ct = default)
    {
        var producer = producerAccessor.GetProducer(KafkaFlowDefaults.ProducerName);
        var value = Encoding.UTF8.GetBytes(payload);

        using var activity = KafkaTracing.StartProducerActivity(topic);

        var headers = new MessageHeaders
        {
            { "event-type", Encoding.UTF8.GetBytes(eventType) },
            { "correlation-id", Encoding.UTF8.GetBytes(correlationId) },
            { "message-id", Encoding.UTF8.GetBytes(messageId.ToString()) },
            { "actor-type", Encoding.UTF8.GetBytes(actorType) },
        };

        if (actorId is not null)
            headers.Add("actor-id", Encoding.UTF8.GetBytes(actorId));

        KafkaTracing.InjectTraceContext(headers);

        try
        {
            await producer.ProduceAsync(topic, correlationId, value, headers);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }
}
