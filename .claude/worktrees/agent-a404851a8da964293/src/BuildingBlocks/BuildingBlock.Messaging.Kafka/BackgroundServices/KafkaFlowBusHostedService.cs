using KafkaFlow;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NovaCore.BuildingBlock.Messaging.Kafka.BackgroundServices;

/// <summary>
/// Starts and stops the KafkaFlow bus with the application lifetime.
/// The bus drives both producers and the configured consumer/typed-handler pipeline.
/// </summary>
public sealed class KafkaFlowBusHostedService(
    IServiceProvider serviceProvider,
    ILogger<KafkaFlowBusHostedService> logger)
    : IHostedService
{
    private IKafkaBus? _bus;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _bus = serviceProvider.CreateKafkaBus();
        await _bus.StartAsync(cancellationToken);
        logger.LogInformation("KafkaFlow bus started");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_bus is null)
            return;

        await _bus.StopAsync();
        logger.LogInformation("KafkaFlow bus stopped");
    }
}
