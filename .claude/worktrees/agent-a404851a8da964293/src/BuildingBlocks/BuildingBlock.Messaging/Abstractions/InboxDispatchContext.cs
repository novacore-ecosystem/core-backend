namespace NovaCore.BuildingBlock.Messaging.Abstractions;

/// <summary>
/// Everything an Inbox-tracked attempt needs: enough to make the dedup/retry decision, and
/// enough (Topic/Payload/Headers) for a background retry to replay the consumer later without
/// the original Kafka message.
/// </summary>
public sealed record InboxDispatchContext(
    Guid MessageId,
    string ConsumerName,
    string Topic,
    string Payload,
    IReadOnlyDictionary<string, string> Headers);
