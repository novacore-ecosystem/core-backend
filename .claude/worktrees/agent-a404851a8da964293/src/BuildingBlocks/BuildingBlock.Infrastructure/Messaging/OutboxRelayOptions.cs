namespace NovaCore.BuildingBlock.Infrastructure.Messaging;

public sealed class OutboxRelayOptions
{
    public const string Section = "Outbox:Relay";

    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);

    public int BatchSize { get; set; } = 10;

    public int MaxRetries { get; set; } = 3;

    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);
}
