using System.Diagnostics;
using System.Text;

using KafkaFlow;

namespace NovaCore.BuildingBlock.Messaging.Kafka.Tracing;

/// <summary>
/// Manual OTel instrumentation for Kafka produce/consume - there's no mature auto-instrumentation
/// for KafkaFlow/Confluent.Kafka, so producer/consumer spans and W3C trace-context propagation
/// (via the "traceparent" message header) are wired by hand here. Registered for export via
/// KafkaTracingExtensions.AddKafkaMessagingTracing.
/// </summary>
public static class KafkaTracing
{
    public const string ActivitySourceName = "NovaCore.Messaging.Kafka";
    private const string TraceParentHeader = "traceparent";

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName, "1.0.0");

    public static Activity? StartProducerActivity(string topic)
    {
        var activity = ActivitySource.StartActivity($"{topic} publish", ActivityKind.Producer);

        activity?.SetTag("messaging.system", "kafka");
        activity?.SetTag("messaging.destination.name", topic);
        activity?.SetTag("messaging.operation", "publish");

        return activity;
    }

    public static Activity? StartConsumerActivity(string topic, IReadOnlyDictionary<string, string> headers)
    {
        var parentContext = ExtractParentContext(headers);

        var activity = parentContext is null
            ? ActivitySource.StartActivity($"{topic} process", ActivityKind.Consumer)
            : ActivitySource.StartActivity(
                $"{topic} process", ActivityKind.Consumer, parentContext.Value);

        activity?.SetTag("messaging.system", "kafka");
        activity?.SetTag("messaging.destination.name", topic);
        activity?.SetTag("messaging.operation", "process");

        return activity;
    }

    /// <summary>Injects the current Activity's W3C traceparent into outgoing Kafka headers.</summary>
    public static void InjectTraceContext(MessageHeaders headers)
    {
        var traceParent = Activity.Current?.Id;
        if (traceParent is null)
            return;

        headers.Add(TraceParentHeader, Encoding.UTF8.GetBytes(traceParent));
    }

    private static ActivityContext? ExtractParentContext(IReadOnlyDictionary<string, string> headers)
    {
        if (!headers.TryGetValue(TraceParentHeader, out var traceParent))
            return null;

        return ActivityContext.TryParse(traceParent, null, out var context)
            ? context
            : null;
    }
}
