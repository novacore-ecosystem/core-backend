namespace NovaCore.BuildingBlock.Messaging.Kafka.Configuration;

/// <summary>
/// Configuration options for Kafka producer and consumer.
/// </summary>
public sealed class KafkaOptions
{
    public const string Section = "Kafka";

    /// <summary>
    /// Kafka broker addresses (comma-separated for multiple brokers).
    /// Example: "localhost:9092" or "kafka1:9092,kafka2:9092,kafka3:9092"
    /// </summary>
    public string BootstrapServers { get; set; } = "localhost:9092";

    /// <summary>
    /// Consumer group ID for the service.
    /// </summary>
    public string GroupId { get; set; } = "default-consumer-group";

    /// <summary>
    /// Enable auto-commit of offsets.
    /// </summary>
    public bool EnableAutoCommit { get; set; } = true;

    /// <summary>
    /// Auto-commit interval in milliseconds.
    /// </summary>
    public int AutoCommitIntervalMs { get; set; } = 5000;

    /// <summary>
    /// Auto offset reset strategy ('earliest' or 'latest').
    /// Default is 'earliest' to start from the beginning of the topic.
    /// </summary>
    public string AutoOffsetReset { get; set; } = "earliest";

    /// <summary>
    /// Session timeout in milliseconds.
    /// </summary>
    public int SessionTimeoutMs { get; set; } = 30000;

    /// <summary>
    /// Request timeout in milliseconds.
    /// </summary>
    public int RequestTimeoutMs { get; set; } = 30000;

    /// <summary>
    /// Number of retries for failed operations.
    /// </summary>
    public int Retries { get; set; } = 3;

    /// <summary>
    /// Enable security (SASL/SSL) if needed.
    /// </summary>
    public bool EnableSecurityProtocol { get; set; } = false;

    /// <summary>
    /// SASL mechanism ('PLAIN', 'SCRAM-SHA-256', 'SCRAM-SHA-512').
    /// </summary>
    public string? SaslMechanism { get; set; }

    /// <summary>
    /// SASL username.
    /// </summary>
    public string? SaslUsername { get; set; }

    /// <summary>
    /// SASL password.
    /// </summary>
    public string? SaslPassword { get; set; }

    /// <summary>
    /// Number of parallel workers processing messages for the consumer.
    /// </summary>
    public int ConsumerWorkersCount { get; set; } = 3;

    /// <summary>
    /// Number of messages buffered ahead of the worker pool.
    /// </summary>
    public int ConsumerBufferSize { get; set; } = 100;
}
