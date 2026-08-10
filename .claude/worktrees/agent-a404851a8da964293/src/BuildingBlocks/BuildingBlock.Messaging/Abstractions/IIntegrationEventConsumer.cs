namespace NovaCore.BuildingBlock.Messaging.Abstractions;

/// <summary>
/// Marker interface for integration event consumers.
/// Consumers are adapters that listen to Kafka topics and dispatch Application Commands/Events.
/// Each service registers consumers for integration events it cares about.
/// </summary>
public interface IIntegrationEventConsumer
{
    /// <summary>
    /// Kafka topics this consumer listens to.
    /// </summary>
    IEnumerable<string> Topics { get; }

    /// <summary>
    /// Process a message received from Kafka.
    /// Implementation should deserialize, validate, and dispatch to application layer.
    /// </summary>
    Task HandleAsync(string message, IReadOnlyDictionary<string, string> headers, CancellationToken ct = default);
}
