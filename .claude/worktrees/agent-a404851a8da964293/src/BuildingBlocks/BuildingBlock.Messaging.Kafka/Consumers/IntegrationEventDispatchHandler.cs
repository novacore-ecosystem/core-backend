using System.Diagnostics;
using System.Text;

using NovaCore.BuildingBlock.Messaging.Kafka.Tracing;
using NovaCore.BuildingBlock.Messaging.Services;

using KafkaFlow;

using Microsoft.Extensions.Logging;

namespace NovaCore.BuildingBlock.Messaging.Kafka.Consumers;

/// <summary>
/// Bridges KafkaFlow's typed handler pipeline into the broker-agnostic
/// IntegrationEventConsumerRegistry. No business logic lives here - only
/// byte/header translation and topic-based dispatch.
/// </summary>
public sealed class IntegrationEventDispatchHandler(
    IntegrationEventConsumerRegistry registry,
    ILogger<IntegrationEventDispatchHandler> logger)
    : IMessageHandler<byte[]>
{
    public async Task Handle(IMessageContext context, byte[] message)
    {
        var topic = context.ConsumerContext.Topic;
        var json = Encoding.UTF8.GetString(message);
        var headers = ConvertHeaders(context.Headers);

        using var activity = KafkaTracing.StartConsumerActivity(topic, headers);

        logger.LogDebug("Dispatching message from topic {Topic}", topic);

        try
        {
            await registry.DispatchAsync(topic, json, headers, context.ConsumerContext.WorkerStopped);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

    private static IReadOnlyDictionary<string, string> ConvertHeaders(IMessageHeaders headers) =>
        headers.ToDictionary(h => h.Key, h => Encoding.UTF8.GetString(h.Value));
}
